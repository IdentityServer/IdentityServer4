// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Logging;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public class TokenRequestValidator : ITokenRequestValidator
    {
        private readonly ILogger _logger;

        private readonly IdentityServerOptions _options;
        private readonly IAuthorizationCodeStore _authorizationCodes;
        private readonly CustomGrantValidator _customGrantValidator;
        private readonly ICustomRequestValidator _customRequestValidator;
        private readonly IRefreshTokenStore _refreshTokens;
        private readonly ScopeValidator _scopeValidator;
        private readonly IEventService _events;
        private readonly IResourceOwnerPasswordValidator _resourceOwnerValidator;
        private readonly IProfileService _profile;

        private ValidatedTokenRequest _validatedRequest;
        
        public TokenRequestValidator(IdentityServerOptions options, IAuthorizationCodeStore authorizationCodes, IRefreshTokenStore refreshTokens, IResourceOwnerPasswordValidator resourceOwnerValidator, IProfileService profile, CustomGrantValidator customGrantValidator, ICustomRequestValidator customRequestValidator, ScopeValidator scopeValidator, IEventService events, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TokenRequestValidator>();

            _options = options;
            _authorizationCodes = authorizationCodes;
            _refreshTokens = refreshTokens;
            _resourceOwnerValidator = resourceOwnerValidator;
            _profile = profile;
            _customGrantValidator = customGrantValidator;
            _customRequestValidator = customRequestValidator;
            _scopeValidator = scopeValidator;
            _events = events;
        }

        public async Task<TokenRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, Client client)
        {
            _logger.LogVerbose("Start token request validation");

            _validatedRequest = new ValidatedTokenRequest();

            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            _validatedRequest.Raw = parameters;
            _validatedRequest.Client = client;
            _validatedRequest.Options = _options;

            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
            if (grantType.IsMissing())
            {
                LogError("Grant type is missing.");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            if (grantType.Length > _options.InputLengthRestrictions.GrantType)
            {
                LogError("Grant type is too long.");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            _validatedRequest.GrantType = grantType;

            // standard grant types
            switch (grantType)
            {
                case OidcConstants.GrantTypes.AuthorizationCode:
                    return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
                case OidcConstants.GrantTypes.ClientCredentials:
                    return await RunValidationAsync(ValidateClientCredentialsRequestAsync, parameters);
                case OidcConstants.GrantTypes.Password:
                    return await RunValidationAsync(ValidateResourceOwnerCredentialRequestAsync, parameters);
                case OidcConstants.GrantTypes.RefreshToken:
                    return await RunValidationAsync(ValidateRefreshTokenRequestAsync, parameters);
            }

            // custom grant type
            var result = await RunValidationAsync(ValidateCustomGrantRequestAsync, parameters);

            if (result.IsError)
            {
                if (result.Error.IsPresent())
                {
                    return result;
                }

                LogError("Unsupported grant_type: " + grantType);
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            return result;
        }

        async Task<TokenRequestValidationResult> RunValidationAsync(Func<NameValueCollection, Task<TokenRequestValidationResult>> validationFunc, NameValueCollection parameters)
        {
            // run standard validation
            var result = await validationFunc(parameters);
            if (result.IsError)
            {
                return result;
            }

            // run custom validation
            var customResult = await _customRequestValidator.ValidateTokenRequestAsync(_validatedRequest);

            if (customResult.IsError)
            {
                var message = "Custom token request validator error";

                if (customResult.Error.IsPresent())
                {
                    message += ": " + customResult.Error;
                }

                LogError(message);
                return customResult;
            }

            LogSuccess();
            return customResult;
        }

        private async Task<TokenRequestValidationResult> ValidateAuthorizationCodeRequestAsync(NameValueCollection parameters)
        {
            _logger.LogVerbose("Start validation of authorization code token request");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.AuthorizationCode &&
                _validatedRequest.Client.Flow != Flows.Hybrid)
            {
                LogError("Client not authorized for code flow");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // validate authorization code
            /////////////////////////////////////////////
            var code = parameters.Get(OidcConstants.TokenRequest.Code);
            if (code.IsMissing())
            {
                var error = "Authorization code is missing.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(null, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (code.Length > _options.InputLengthRestrictions.AuthorizationCode)
            {
                var error = "Authorization code is too long.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(null, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.AuthorizationCodeHandle = code;

            var authZcode = await _authorizationCodes.GetAsync(code);
            if (authZcode == null)
            {
                LogError("Invalid authorization code: " + code);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, "Invalid handle");

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            await _authorizationCodes.RemoveAsync(code);

            /////////////////////////////////////////////
            // populate session id
            /////////////////////////////////////////////
            if (authZcode.SessionId.IsPresent())
            {
                _validatedRequest.SessionId = authZcode.SessionId;
            }

            /////////////////////////////////////////////
            // validate client binding
            /////////////////////////////////////////////
            if (authZcode.Client.ClientId != _validatedRequest.Client.ClientId)
            {
                LogError(string.Format("Client {0} is trying to use a code from client {1}", _validatedRequest.Client.ClientId, authZcode.Client.ClientId));
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, "Invalid client binding");

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // validate code expiration
            /////////////////////////////////////////////
            if (authZcode.CreationTime.HasExceeded(_validatedRequest.Client.AuthorizationCodeLifetime))
            {
                var error = "Authorization code is expired";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.AuthorizationCode = authZcode;

            /////////////////////////////////////////////
            // validate redirect_uri
            /////////////////////////////////////////////
            var redirectUri = parameters.Get(OidcConstants.TokenRequest.RedirectUri);
            if (redirectUri.IsMissing())
            {
                var error = "Redirect URI is missing.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }

            if (redirectUri.Equals(_validatedRequest.AuthorizationCode.RedirectUri, StringComparison.Ordinal) == false)
            {
                var error = "Invalid redirect_uri: " + redirectUri;
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // validate scopes are present
            /////////////////////////////////////////////
            if (_validatedRequest.AuthorizationCode.RequestedScopes == null ||
                !_validatedRequest.AuthorizationCode.RequestedScopes.Any())
            {
                var error = "Authorization code has no associated scopes.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(OidcConstants.TokenErrors.InvalidRequest);
            }


            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var isActiveCtx = new IsActiveContext(_validatedRequest.AuthorizationCode.Subject, _validatedRequest.Client);
            await _profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                var error = "User has been disabled: " + _validatedRequest.AuthorizationCode.Subject;
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(OidcConstants.TokenErrors.InvalidRequest);
            }

            _logger.LogInformation("Validation of authorization code token request success");
            await RaiseSuccessfulAuthorizationCodeRedeemedEventAsync();

            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateClientCredentialsRequestAsync(NameValueCollection parameters)
        {
            _logger.LogVerbose("Start client credentials token request validation");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.ClientCredentials)
            {
                if (_validatedRequest.Client.AllowClientCredentialsOnly == false)
                {
                    LogError("Client not authorized for client credentials flow");
                    return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
                }
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                LogError("Invalid scopes.");
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            if (_validatedRequest.ValidatedScopes.ContainsOpenIdScopes)
            {
                LogError("Client cannot request OpenID scopes in client credentials flow");
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            if (_validatedRequest.ValidatedScopes.ContainsOfflineAccessScope)
            {
                LogError("Client cannot request a refresh token in client credentials flow");
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            _logger.LogInformation("Client credentials token request validation success");
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateResourceOwnerCredentialRequestAsync(NameValueCollection parameters)
        {
            _logger.LogInformation("Start password token request validation");

            // if we've disabled local authentication, then fail
            if (_options.AuthenticationOptions.EnableLocalLogin == false ||
                _validatedRequest.Client.EnableLocalLogin == false)
            {
                LogError("EnableLocalLogin is disabled, failing with UnsupportedGrantType");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.ResourceOwner)
            {
                LogError("Client not authorized for resource owner flow");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                LogError("Invalid scopes.");
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // check resource owner credentials
            /////////////////////////////////////////////
            var userName = parameters.Get(OidcConstants.TokenRequest.UserName);
            var password = parameters.Get(OidcConstants.TokenRequest.Password);

            if (userName.IsMissing() || password.IsMissing())
            {
                LogError("Username or password missing.");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (userName.Length > _options.InputLengthRestrictions.UserName ||
                password.Length > _options.InputLengthRestrictions.Password)
            {
                LogError("Username or password too long.");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.UserName = userName;

            
            /////////////////////////////////////////////
            // authenticate user
            /////////////////////////////////////////////
            var resourceOwnerResult = await _resourceOwnerValidator.ValidateAsync(userName, password, _validatedRequest);

            if (resourceOwnerResult.IsError)
            {
                var error = "invalid_username_or_password";

                if (resourceOwnerResult.Error.IsPresent())
                {
                    error = resourceOwnerResult.Error;
                }

                LogError("User authentication failed: " + error);
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant, error);
            }

            if (resourceOwnerResult.Principal == null)
            {
                var error = "User authentication failed: no principal returned";
                LogError(error);
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.UserName = userName;
            _validatedRequest.Subject = resourceOwnerResult.Principal;

            await RaiseSuccessfulResourceOwnerAuthenticationEventAsync(userName, resourceOwnerResult.Principal.GetSubjectId());
            _logger.LogInformation("Password token request validation success.");
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateRefreshTokenRequestAsync(NameValueCollection parameters)
        {
            _logger.LogVerbose("Start validation of refresh token request");

            var refreshTokenHandle = parameters.Get(OidcConstants.TokenRequest.RefreshToken);
            if (refreshTokenHandle.IsMissing())
            {
                var error = "Refresh token is missing";
                LogError(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(null, error);

                return Invalid(OidcConstants.TokenErrors.InvalidRequest);
            }

            if (refreshTokenHandle.Length > _options.InputLengthRestrictions.RefreshToken)
            {
                var error = "Refresh token too long";
                LogError(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(null, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.RefreshTokenHandle = refreshTokenHandle;

            /////////////////////////////////////////////
            // check if refresh token is valid
            /////////////////////////////////////////////
            var refreshToken = await _refreshTokens.GetAsync(refreshTokenHandle);
            if (refreshToken == null)
            {
                var error = "Refresh token is invalid";
                LogWarn(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // check if refresh token has expired
            /////////////////////////////////////////////
            if (refreshToken.CreationTime.HasExceeded(refreshToken.LifeTime))
            {
                var error = "Refresh token has expired";
                LogWarn(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                await _refreshTokens.RemoveAsync(refreshTokenHandle);
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // check if client belongs to requested refresh token
            /////////////////////////////////////////////
            if (_validatedRequest.Client.ClientId != refreshToken.ClientId)
            {
                LogError(string.Format("Client {0} tries to refresh token belonging to client {1}", _validatedRequest.Client.ClientId, refreshToken.ClientId));
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, "Invalid client binding");

                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // check if client still has offline_access scope
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowAccessToAllScopes)
            {
                if (!_validatedRequest.Client.AllowedScopes.Contains(Constants.StandardScopes.OfflineAccess))
                {
                    var error = "Client does not have access to offline_access scope anymore";
                    LogError(error);
                    await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                    return Invalid(OidcConstants.TokenErrors.InvalidGrant);
                }
            }

            _validatedRequest.RefreshToken = refreshToken;

            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var principal = IdentityServerPrincipal.FromSubjectId(_validatedRequest.RefreshToken.SubjectId, refreshToken.AccessToken.Claims);

            var isActiveCtx = new IsActiveContext(principal, _validatedRequest.Client);
            await _profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                var error = "User has been disabled: " + _validatedRequest.RefreshToken.SubjectId;
                LogError(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                return Invalid(OidcConstants.TokenErrors.InvalidRequest);
            }

            _logger.LogInformation("Validation of refresh token request success");
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateCustomGrantRequestAsync(NameValueCollection parameters)
        {
            _logger.LogVerbose("Start validation of custom grant token request");

            /////////////////////////////////////////////
            // check if client is authorized for custom grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.Custom)
            {
                LogError("Client not registered for custom grant type");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is allowed grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.AllowAccessToAllCustomGrantTypes == false)
            {
                if (!_validatedRequest.Client.AllowedCustomGrantTypes.Contains(_validatedRequest.GrantType))
                {
                    LogError("Client does not have the custom grant type in the allowed list, therefore requested grant is not allowed.");
                    return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
                }
            }

            /////////////////////////////////////////////
            // check if a validator is registered for the grant type
            /////////////////////////////////////////////
            if (!_customGrantValidator.GetAvailableGrantTypes().Contains(_validatedRequest.GrantType, StringComparer.Ordinal))
            {
                LogError("No validator is registered for the grant type.");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                LogError("Invalid scopes.");
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // validate custom grant type
            /////////////////////////////////////////////
            var result = await _customGrantValidator.ValidateAsync(_validatedRequest);

            if (result == null)
            {
                LogError("Invalid custom grant.");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (result.IsError)
            {
                if (result.Error.IsPresent())
                {
                    LogError("Invalid custom grant: " + result.Error);
                    return Invalid(result.Error);
                }
                else
                {
                    LogError("Invalid custom grant.");
                    return Invalid(OidcConstants.TokenErrors.InvalidGrant);
                }
            }

            if (result.Principal != null)
            {
                _validatedRequest.Subject = result.Principal;
            }

            _logger.LogInformation("Validation of custom grant token request success");
            return Valid();
        }

        private async Task<bool> ValidateRequestedScopesAsync(NameValueCollection parameters)
        {
            var scopes = parameters.Get(OidcConstants.TokenRequest.Scope);
            if (scopes.IsMissingOrTooLong(_options.InputLengthRestrictions.Scope))
            {
                _logger.LogWarning("Scopes missing or too long");
                return false;
            }

            var requestedScopes = scopes.ParseScopesString();

            if (requestedScopes == null)
            {
                return false;
            }

            if (!_scopeValidator.AreScopesAllowed(_validatedRequest.Client, requestedScopes))
            {
                return false;
            }

            if (!await _scopeValidator.AreScopesValidAsync(requestedScopes))
            {
                return false;
            }

            _validatedRequest.Scopes = requestedScopes;
            _validatedRequest.ValidatedScopes = _scopeValidator;
            return true;
        }

        private TokenRequestValidationResult Valid()
        {
            return new TokenRequestValidationResult(_validatedRequest);
        }

        private TokenRequestValidationResult Invalid(string error)
        {
            return new TokenRequestValidationResult(error);
        }

        private TokenRequestValidationResult Invalid(string error, string errorDescription)
        {
            return new TokenRequestValidationResult(error, errorDescription);
        }

        private void LogError(string message)
        {
            _logger.LogError(LogEvent(message));
        }

        private void LogWarn(string message)
        {
            _logger.LogWarning(LogEvent(message));
        }

        private void LogSuccess()
        {
            _logger.LogInformation(LogEvent("Token request validation success"));
        }

        private string LogEvent(string message)
        {
            var validationLog = new TokenRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(validationLog);

            return string.Format("{0}\n {1}", message, json);
        }

        // todo
        //private Func<string> LogEvent(string message)
        //{
        //    return () =>
        //    {
        //        var validationLog = new TokenRequestValidationLog(_validatedRequest);
        //        var json = LogSerializer.Serialize(validationLog);

        //        return string.Format("{0}\n {1}", message, json);
        //    };
        //}

        private async Task RaiseSuccessfulResourceOwnerAuthenticationEventAsync(string userName, string subjectId)
        {
            await _events.RaiseSuccessfulResourceOwnerFlowAuthenticationEventAsync(userName, subjectId);
        }

        private async Task RaiseFailedResourceOwnerAuthenticationEventAsync(string userName, string error)
        {
            await _events.RaiseFailedResourceOwnerFlowAuthenticationEventAsync(userName, error);
        }

        private async Task RaiseFailedAuthorizationCodeRedeemedEventAsync(string handle, string error)
        {
            await _events.RaiseFailedAuthorizationCodeRedeemedEventAsync(_validatedRequest.Client, handle, error);
        }

        private async Task RaiseSuccessfulAuthorizationCodeRedeemedEventAsync()
        {
            await _events.RaiseSuccessAuthorizationCodeRedeemedEventAsync(_validatedRequest.Client, _validatedRequest.AuthorizationCodeHandle);
        }

        private async Task RaiseRefreshTokenRefreshFailureEventAsync(string handle, string error)
        {
            await _events.RaiseFailedRefreshTokenRefreshEventAsync(_validatedRequest.Client, handle, error);
        }
    }
}