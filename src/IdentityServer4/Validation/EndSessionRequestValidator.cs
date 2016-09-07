// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Logging;
using IdentityServer4.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IdentityServer4.Validation
{
    internal class EndSessionRequestValidator : IEndSessionRequestValidator
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly ITokenValidator _tokenValidator;
        private readonly IRedirectUriValidator _uriValidator;
        private readonly ISessionIdService _sessionId;
        private readonly IClientSessionService _clientSession;
        private readonly IClientStore _clientStore;
        private readonly IHttpContextAccessor _context;

        public EndSessionRequestValidator(
            IHttpContextAccessor context,
            IdentityServerOptions options, 
            ITokenValidator tokenValidator, 
            IRedirectUriValidator uriValidator,
            ISessionIdService sessionId,
            IClientSessionService clientSession,
            IClientStore clientStore,
            ILogger<EndSessionRequestValidator> logger)
        {
            _context = context;
            _options = options;
            _tokenValidator = tokenValidator;
            _uriValidator = uriValidator;
            _sessionId = sessionId;
            _clientSession = clientSession;
            _clientStore = clientStore;
            _logger = logger;
        }

        public async Task<EndSessionValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            _logger.LogDebug("Start end session request validation");

            var isAuthenticated = subject != null &&
                subject.Identity != null &&
                subject.Identity.IsAuthenticated;

            if (!isAuthenticated && _options.AuthenticationOptions.RequireAuthenticatedUserForSignOutMessage)
            {
                _logger.LogWarning("User is anonymous. Ignoring end session parameters");
                return Invalid();
            }

            var validatedRequest = new ValidatedEndSessionRequest()
            {
                Raw = parameters,
            };

            var idTokenHint = parameters.Get(OidcConstants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token - no need to validate token life time
                var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint, null, false);
                if (tokenValidationResult.IsError)
                {
                    LogWarning(validatedRequest, "Error validating id token hint.");
                    return Invalid();
                }

                validatedRequest.Client = tokenValidationResult.Client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
                if (subClaim != null && isAuthenticated)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        LogWarning(validatedRequest, "Current user does not match identity token");
                        return Invalid();
                    }

                    validatedRequest.Subject = subject;
                }

                var redirectUri = parameters.Get(OidcConstants.EndSessionRequest.PostLogoutRedirectUri);
                if (redirectUri.IsPresent())
                {
                    if (await _uriValidator.IsPostLogoutRedirectUriValidAsync(redirectUri, validatedRequest.Client) == false)
                    {
                        LogWarning(validatedRequest, "Invalid post logout URI");
                        return Invalid();
                    }

                    validatedRequest.PostLogOutUri = redirectUri;
                }
                else if (validatedRequest.Client.PostLogoutRedirectUris.Count == 1)
                {
                    validatedRequest.PostLogOutUri = validatedRequest.Client.PostLogoutRedirectUris.First();
                }

                if (validatedRequest.PostLogOutUri != null)
                {
                    var state = parameters.Get(OidcConstants.EndSessionRequest.State);
                    if (state.IsPresent())
                    {
                        validatedRequest.State = state;
                    }
                }
            }

            LogSuccess(validatedRequest);

            return new EndSessionValidationResult()
            {
                ValidatedRequest = validatedRequest,
                IsError = false
            };
        }

        private EndSessionValidationResult Invalid()
        {
            return new EndSessionValidationResult
            {
                IsError = true,
                Error = "Invalid request"
            };
        }

        private void LogWarning(ValidatedEndSessionRequest request,  string message)
        {
            var log = new EndSessionRequestValidationLog(request);
            _logger.LogWarning(message + "\n{details}", log);
        }

        private void LogSuccess(ValidatedEndSessionRequest request)
        {
            var log = new EndSessionRequestValidationLog(request);
            _logger.LogInformation("End session request validation success\n{details}", log);
        }

        public async Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(NameValueCollection parameters)
        {
            var result = new EndSessionCallbackValidationResult()
            {
                IsError = true
            };

            result.LogoutId = parameters[_options.UserInteractionOptions.LogoutIdParameter];

            var sid = ValidateSid(parameters);
            if (sid == null)
            {
                result.Error = "Invalid session id";
            }
            else
            {
                result.IsError = false;
                result.ClientLogoutUrls = await GetClientEndSessionUrlsAsync(sid);
            }

            return result;
        }

        private string ValidateSid(NameValueCollection parameters)
        {
            var sidCookie = _sessionId.GetCookieValue();
            if (sidCookie != null)
            {
                var sid = parameters[OidcConstants.EndSessionRequest.Sid];
                if (sid != null)
                {
                    if (TimeConstantComparer.IsEqual(sid, sidCookie))
                    {
                        _logger.LogDebug("sid validation successful");
                        return sid;
                    }
                    else
                    {
                        _logger.LogError("sid in query string does not match sid from cookie");
                    }

                }
                else
                {
                    _logger.LogError("No sid in query string");
                }
            }
            else
            {
                _logger.LogError("No sid in cookie");
            }

            return null;
        }

        private async Task<IEnumerable<string>> GetClientEndSessionUrlsAsync(string sid)
        {
            // read client list to get URLs for client logout endpoints
            var clientIds = _clientSession.GetClientListFromCookie();

            var urls = new List<string>();
            foreach (var clientId in clientIds)
            {
                var client = await _clientStore.FindClientByIdAsync(clientId);

                if (client != null && client.LogoutUri.IsPresent())
                {
                    var url = client.LogoutUri;

                    // add session id if required
                    if (client.LogoutSessionRequired)
                    {
                        url = url.AddQueryString(OidcConstants.EndSessionRequest.Sid, sid);
                        //TODO: update sid to OidcConstants when idmodel released
                        url = url.AddQueryString("iss", _context.HttpContext.GetIssuerUri());
                    }

                    urls.Add(url);
                }
            }

            if (urls.Any())
            {
                var msg = urls.Aggregate((x, y) => x + ", " + y);
                _logger.LogDebug("Client end session iframe URLs: {0}", msg);
            }
            else
            {
                _logger.LogDebug("No client end session iframe URLs");
            }

            return urls;
        }
    }
}