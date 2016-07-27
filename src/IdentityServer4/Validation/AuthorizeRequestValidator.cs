// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Logging;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    internal class AuthorizeRequestValidator : IAuthorizeRequestValidator
    {
        private readonly IdentityServerOptions _options;
        private readonly IClientStore _clients;
        private readonly ICustomRequestValidator _customValidator;
        private readonly IRedirectUriValidator _uriValidator;
        private readonly ScopeValidator _scopeValidator;
        private readonly SessionCookie _sessionCookie;
        private readonly ILogger _logger;

        public AuthorizeRequestValidator(
            IdentityServerOptions options, 
            IClientStore clients, 
            ICustomRequestValidator customValidator, 
            IRedirectUriValidator uriValidator, 
            ScopeValidator scopeValidator,
            SessionCookie sessionCookie,
            ILogger<AuthorizeRequestValidator> logger)
        {
            _options = options;
            _clients = clients;
            _customValidator = customValidator;
            _uriValidator = uriValidator;
            _scopeValidator = scopeValidator;
            _sessionCookie = sessionCookie;
            _logger = logger;
        }

        public async Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null)
        {
            _logger.LogDebug("Start authorize request protocol validation");

            var request = new ValidatedAuthorizeRequest
            {
                Options = _options,
                Subject = subject ?? Principal.Anonymous
            };
            
            if (parameters == null)
            {
                _logger.LogCritical("Parameters are null.");
                throw new ArgumentNullException(nameof(parameters));
            }

            request.Raw = parameters;

            // validate client_id and redirect_uri
            var clientResult = await ValidateClientAsync(request);
            if (clientResult.IsError)
            {
                return clientResult;
            }

            // state, response_type, response_mode
            var mandatoryResult = ValidateCoreParameters(request);
            if (mandatoryResult.IsError)
            {
                return mandatoryResult;
            }

            // scope, scope restrictions and plausability
            var scopeResult = await ValidateScopeAsync(request);
            if (scopeResult.IsError)
            {
                return scopeResult;
            }

            // nonce, prompt, acr_values, login_hint etc.
            var optionalResult = ValidateOptionalParameters(request);
            if (optionalResult.IsError)
            {
                return optionalResult;
            }

            // custom validator
            _logger.LogDebug("Calling into custom validator: {type}", _customValidator.GetType().FullName);
            var customResult = await _customValidator.ValidateAuthorizeRequestAsync(request);

            if (customResult.IsError)
            {
                LogError("Error in custom validation: " + customResult.Error, request);
                return Invalid(request, customResult.ErrorType, customResult.Error);
            }

            LogSuccess(request);
            return Valid(request);
        }

        async Task<AuthorizeRequestValidationResult> ValidateClientAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // client_id must be present
            /////////////////////////////////////////////////////////
            var clientId = request.Raw.Get(OidcConstants.AuthorizeRequest.ClientId);
            if (clientId.IsMissingOrTooLong(_options.InputLengthRestrictions.ClientId))
            {
                LogError("client_id is missing or too long", request);
                return Invalid(request);
            }

            request.ClientId = clientId;


            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            var redirectUri = request.Raw.Get(OidcConstants.AuthorizeRequest.RedirectUri);

            if (redirectUri.IsMissingOrTooLong(_options.InputLengthRestrictions.RedirectUri))
            {
                LogError("redirect_uri is missing or too long", request);
                return Invalid(request);
            }

            Uri uri;
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
            {
                LogError("malformed redirect_uri: " + redirectUri, request);
                return Invalid(request);
            }

            request.RedirectUri = redirectUri;


            //////////////////////////////////////////////////////////
            // check for valid client
            //////////////////////////////////////////////////////////
            var client = await _clients.FindClientByIdAsync(request.ClientId);
            if (client == null || client.Enabled == false)
            {
                LogError("Unknown client or not enabled: " + request.ClientId, request);
                return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnauthorizedClient);
            }

            request.Client = client;

            //////////////////////////////////////////////////////////
            // check if redirect_uri is valid
            //////////////////////////////////////////////////////////
            if (await _uriValidator.IsRedirectUriValidAsync(request.RedirectUri, request.Client) == false)
            {
                LogError("Invalid redirect_uri: " + request.RedirectUri, request);
                return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnauthorizedClient);
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidateCoreParameters(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check state
            //////////////////////////////////////////////////////////
            var state = request.Raw.Get(OidcConstants.AuthorizeRequest.State);
            if (state.IsPresent())
            {
                request.State = state;
            }

            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = request.Raw.Get(OidcConstants.AuthorizeRequest.ResponseType);
            if (responseType.IsMissing())
            {
                LogError("Missing response_type", request);
                return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnsupportedResponseType);
            }

            if (!Constants.SupportedResponseTypes.Contains(responseType))
            {
                LogError("Response type not supported: " + responseType, request);
                return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnsupportedResponseType);
            }

            request.ResponseType = responseType;

            //////////////////////////////////////////////////////////
            // match response_type to grant type
            //////////////////////////////////////////////////////////
            request.GrantType = Constants.ResponseTypeToGrantTypeMapping[request.ResponseType];

            //////////////////////////////////////////////////////////
            // check if flow is allowed at authorize endpoint
            //////////////////////////////////////////////////////////
            if (!Constants.AllowedGrantTypesForAuthorizeEndpoint.Contains(request.GrantType))
            {
                LogError("Invalid grant type", request);
                return Invalid(request);
            }

            //////////////////////////////////////////////////////////
            // check if PKCE is required and validate parameters
            //////////////////////////////////////////////////////////
            if (request.GrantType == GrantType.AuthorizationCode || request.GrantType == GrantType.Hybrid)
            {
                if (request.Client.RequirePkce)
                {
                    _logger.LogDebug("Client requires a proof key for code exchange. Starting PKCE validation");

                    /////////////////////////////////////////////////////////////////////////////
                    // validate code_challenge and code_challenge_method
                    /////////////////////////////////////////////////////////////////////////////
                    var proofKeyResult = ValidatePkceParameters(request);
                    if (proofKeyResult.IsError)
                    {
                        return proofKeyResult;
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check response_mode parameter and set response_mode
            //////////////////////////////////////////////////////////

            // set default response mode for flow first
            request.ResponseMode = Constants.AllowedResponseModesForGrantType[request.GrantType].First();

            // check if response_mode parameter is present and valid
            var responseMode = request.Raw.Get(OidcConstants.AuthorizeRequest.ResponseMode);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (Constants.AllowedResponseModesForGrantType[request.GrantType].Contains(responseMode))
                    {
                        request.ResponseMode = responseMode;
                    }
                    else
                    {
                        LogError("Invalid response_mode for flow: " + responseMode, request);
                        return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnsupportedResponseType);
                    }
                }
                else
                {
                    LogError("Unsupported response_mode: " + responseMode, request);
                    return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnsupportedResponseType);
                }
            }

            
            //////////////////////////////////////////////////////////
            // check if grant type is allowed for client
            //////////////////////////////////////////////////////////
            if (!request.Client.AllowedGrantTypes.Contains(request.GrantType))
            {
                LogError("Invalid grant type for client: " + request.GrantType, request);
                return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check if response type contains an access token, 
            // and if client is allowed to request access token via browser
            //////////////////////////////////////////////////////////
            var responseTypes = responseType.FromSpaceSeparatedString();
            if (responseTypes.Contains(OidcConstants.ResponseTypes.Token))
            {
                if (!request.Client.AllowAccessTokensViaBrowser)
                {
                    LogError("Client requested access token - but client is not configured to receive access tokens via browser", request);
                    return Invalid(request);
                }
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidatePkceParameters(ValidatedAuthorizeRequest request)
        {
            var fail = Invalid(request, ErrorTypes.Client);

            var codeChallenge = request.Raw.Get(OidcConstants.AuthorizeRequest.CodeChallenge);
            if (codeChallenge.IsMissing())
            {
                LogError("code_challenge is missing", request);
                fail.ErrorDescription = "code challenge required";
                return fail;
            }

            if (codeChallenge.Length < _options.InputLengthRestrictions.CodeChallengeMinLength ||
                codeChallenge.Length > _options.InputLengthRestrictions.CodeChallengeMaxLength)
            {
                LogError("code_challenge is either too short or too long", request);
                return fail;
            }

            request.CodeChallenge = codeChallenge;

            var codeChallengeMethod = request.Raw.Get(OidcConstants.AuthorizeRequest.CodeChallengeMethod);
            if (codeChallengeMethod.IsMissing())
            {
                _logger.LogDebug("Missing code_challenge_method, defaulting to plain");
                codeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain;
            }

            if (!Constants.SupportedCodeChallengeMethods.Contains(codeChallengeMethod))
            {
                LogError("Unsupported code_challenge_method: " + codeChallengeMethod, request);
                fail.ErrorDescription = "transform algorithm not supported";
                return fail;
            }

            request.CodeChallengeMethod = codeChallengeMethod;

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> ValidateScopeAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = request.Raw.Get(OidcConstants.AuthorizeRequest.Scope);
            if (scope.IsMissing())
            {
                LogError("scope is missing", request);
                return Invalid(request, ErrorTypes.Client);
            }

            if (scope.Length > _options.InputLengthRestrictions.Scope)
            {
                LogError("scopes too long.", request);
                return Invalid(request, ErrorTypes.Client);
            }

            request.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();

            if (request.RequestedScopes.Contains(Constants.StandardScopes.OpenId))
            {
                request.IsOpenIdRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scope vs response_type plausability
            //////////////////////////////////////////////////////////
            var requirement = Constants.ResponseTypeToScopeRequirement[request.ResponseType];
            if (requirement == Constants.ScopeRequirement.Identity ||
                requirement == Constants.ScopeRequirement.IdentityOnly)
            {
                if (request.IsOpenIdRequest == false)
                {
                    LogError("response_type requires the openid scope", request);
                    return Invalid(request, ErrorTypes.Client);
                }
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported and check for resource scopes
            //////////////////////////////////////////////////////////
            if (await _scopeValidator.AreScopesValidAsync(request.RequestedScopes) == false)
            {
                return Invalid(request, ErrorTypes.Client, OidcConstants.AuthorizeErrors.InvalidScope);
            }

            if (_scopeValidator.ContainsOpenIdScopes && !request.IsOpenIdRequest)
            {
                LogError("Identity related scope requests, but no openid scope", request);
                return Invalid(request, ErrorTypes.Client, OidcConstants.AuthorizeErrors.InvalidScope);
            }

            if (_scopeValidator.ContainsResourceScopes)
            {
                request.IsResourceRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            if (!_scopeValidator.AreScopesAllowed(request.Client, request.RequestedScopes))
            {
                return Invalid(request, ErrorTypes.User, OidcConstants.AuthorizeErrors.UnauthorizedClient);
            }

            request.ValidatedScopes = _scopeValidator;

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            if (!_scopeValidator.IsResponseTypeValid(request.ResponseType))
            {
                return Invalid(request, ErrorTypes.Client, OidcConstants.AuthorizeErrors.InvalidScope);
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidateOptionalParameters(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check nonce
            //////////////////////////////////////////////////////////
            var nonce = request.Raw.Get(OidcConstants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                if (nonce.Length > _options.InputLengthRestrictions.Nonce)
                {
                    LogError("Nonce too long", request);
                    return Invalid(request, ErrorTypes.Client);
                }

                request.Nonce = nonce;
            }
            else
            {
                if (request.GrantType == GrantType.Implicit ||
                    request.GrantType == GrantType.Hybrid)
                {
                    // only openid requests require nonce
                    if (request.IsOpenIdRequest)
                    {
                        LogError("Nonce required for implicit and hybrid flow with openid scope", request);
                        return Invalid(request, ErrorTypes.Client);
                    }
                }
            }


            //////////////////////////////////////////////////////////
            // check prompt
            //////////////////////////////////////////////////////////
            var prompt = request.Raw.Get(OidcConstants.AuthorizeRequest.Prompt);
            if (prompt.IsPresent())
            {
                if (Constants.SupportedPromptModes.Contains(prompt))
                {
                    request.PromptMode = prompt;
                }
                else
                {
                    _logger.LogDebug("Unsupported prompt mode - ignored: " + prompt);
                }
            }

            //////////////////////////////////////////////////////////
            // check ui locales
            //////////////////////////////////////////////////////////
            var uilocales = request.Raw.Get(OidcConstants.AuthorizeRequest.UiLocales);
            if (uilocales.IsPresent())
            {
                if (uilocales.Length > _options.InputLengthRestrictions.UiLocale)
                {
                    LogError("UI locale too long", request);
                    return Invalid(request, ErrorTypes.Client);
                }

                request.UiLocales = uilocales;
            }

            //////////////////////////////////////////////////////////
            // check display
            //////////////////////////////////////////////////////////
            var display = request.Raw.Get(OidcConstants.AuthorizeRequest.Display);
            if (display.IsPresent())
            {
                if (Constants.SupportedDisplayModes.Contains(display))
                {
                    request.DisplayMode = display;
                }

                _logger.LogDebug("Unsupported display mode - ignored: " + display);
            }

            //////////////////////////////////////////////////////////
            // check max_age
            //////////////////////////////////////////////////////////
            var maxAge = request.Raw.Get(OidcConstants.AuthorizeRequest.MaxAge);
            if (maxAge.IsPresent())
            {
                int seconds;
                if (int.TryParse(maxAge, out seconds))
                {
                    if (seconds >= 0)
                    {
                        request.MaxAge = seconds;
                    }
                    else
                    {
                        LogError("Invalid max_age.", request);
                        return Invalid(request, ErrorTypes.Client);
                    }
                }
                else
                {
                    LogError("Invalid max_age.", request);
                    return Invalid(request, ErrorTypes.Client);
                }
            }

            //////////////////////////////////////////////////////////
            // check login_hint
            //////////////////////////////////////////////////////////
            var loginHint = request.Raw.Get(OidcConstants.AuthorizeRequest.LoginHint);
            if (loginHint.IsPresent())
            {
                if (loginHint.Length > _options.InputLengthRestrictions.LoginHint)
                {
                    LogError("Login hint too long", request);
                    return Invalid(request, ErrorTypes.Client);
                }

                request.LoginHint = loginHint;
            }

            //////////////////////////////////////////////////////////
            // check acr_values
            //////////////////////////////////////////////////////////
            var acrValues = request.Raw.Get(OidcConstants.AuthorizeRequest.AcrValues);
            if (acrValues.IsPresent())
            {
                if (acrValues.Length > _options.InputLengthRestrictions.AcrValues)
                {
                    LogError("Acr values too long", request);
                    return Invalid(request, ErrorTypes.Client);
                }

                request.AuthenticationContextReferenceClasses = acrValues.FromSpaceSeparatedString().Distinct().ToList();
            }

            //////////////////////////////////////////////////////////
            // check session cookie
            //////////////////////////////////////////////////////////
            if (_options.Endpoints.EnableCheckSessionEndpoint && 
                request.Subject.Identity.IsAuthenticated)
            {
                var sessionId = _sessionCookie.GetOrCreateSessionId();
                if (sessionId.IsPresent())
                {
                    request.SessionId = sessionId;
                }
                else
                {
                    LogError("Check session endpoint enabled, but SessionId is missing", request);
                }
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult Invalid(ValidatedAuthorizeRequest request, ErrorTypes errorType = ErrorTypes.User, string error = OidcConstants.AuthorizeErrors.InvalidRequest)
        {
            var result = new AuthorizeRequestValidationResult
            {
                IsError = true,
                Error = error,
                ErrorType = errorType,
                ValidatedRequest = request
            };

            return result;
        }

        private AuthorizeRequestValidationResult Valid(ValidatedAuthorizeRequest request)
        {
            var result = new AuthorizeRequestValidationResult
            {
                IsError = false,
                ValidatedRequest = request
            };

            return result;
        }

        private void LogError(string message, ValidatedAuthorizeRequest request)
        {
            var details = new AuthorizeRequestValidationLog(request);
            _logger.LogError(message + "\n{validationDetails}", details);
        }

        private void LogSuccess(ValidatedAuthorizeRequest request)
        {
            var details = new AuthorizeRequestValidationLog(request);
            _logger.LogInformation("Authorize request validation success\n{validationDetails}", details);
        }
    }
}