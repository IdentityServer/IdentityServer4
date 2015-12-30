/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Resources;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    class AuthorizeInteractionResponseGenerator : IAuthorizeInteractionResponseGenerator
    {
        private readonly ILogger<AuthorizeInteractionResponseGenerator> _logger;
        private readonly IdentityServerOptions _options;
        private readonly IConsentService _consent;
        private readonly IUserService _users;
        private readonly ILocalizationService _localizationService;

        public AuthorizeInteractionResponseGenerator(
            ILogger<AuthorizeInteractionResponseGenerator> logger, 
            IdentityServerOptions options, 
            IConsentService consent, 
            IUserService users, 
            ILocalizationService localizationService)
        {
            _logger = logger;
            _options = options;
            _consent = consent;
            _users = users;
            _localizationService = localizationService;
        }

        public async Task<LoginInteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            if (request.PromptMode == Constants.PromptModes.Login)
            {
                // remove prompt so when we redirect back in from login page
                // we won't think we need to force a prompt again
                request.Raw.Remove(Constants.AuthorizeRequest.Prompt);

                _logger.LogInformation("Redirecting to login page because of prompt=login");

                return new LoginInteractionResponse() { IsLogin = true };
            }

            // unauthenticated user
            var isAuthenticated = user.Identity.IsAuthenticated;
            
            // user de-activated
            bool isActive = false;

            if (isAuthenticated)
            {
                var isActiveCtx = new IsActiveContext(user, request.Client);
                await _users.IsActiveAsync(isActiveCtx);
                
                isActive = isActiveCtx.IsActive;
            }

            if (!isAuthenticated || !isActive)
            {
                if (!isAuthenticated) _logger.LogInformation("User is not authenticated.");
                else if (!isActive) _logger.LogInformation("User is not active.");

                // prompt=none means user must be signed in already
                if (request.PromptMode == Constants.PromptModes.None)
                {
                    _logger.LogInformation("prompt=none was requested but user is not authenticated/active.");

                    return new LoginInteractionResponse
                    {
                        Error = new AuthorizeError
                        {
                            ErrorType = ErrorTypes.Client,
                            Error = Constants.AuthorizeErrors.LoginRequired,
                            ResponseMode = request.ResponseMode,
                            ErrorUri = request.RedirectUri,
                            State = request.State
                        }
                    };
                }

                return new LoginInteractionResponse() { IsLogin = true };
            }

            // check current idp
            var currentIdp = user.GetIdentityProvider();

            // check if idp login hint matches current provider
            var idp = request.GetIdP();
            if (idp.IsPresent())
            {
                if (idp != currentIdp)
                {
                    _logger.LogInformation("Current IdP is not the requested IdP. Redirecting to login");
                    _logger.LogInformation("Current: {0} -- Requested: {1}", currentIdp, idp);

                    return new LoginInteractionResponse() { IsLogin = true };
                }
            }

            // check authentication freshness
            if (request.MaxAge.HasValue)
            {
                var authTime = user.GetAuthenticationTime();
                if (DateTimeOffsetHelper.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    _logger.LogInformation("Requested MaxAge exceeded.");

                    return new LoginInteractionResponse() { IsLogin = true };
                }
            }

            // update validated request with user
            request.Subject = user;

            // check idp restrictions
            if (request.Client.IdentityProviderRestrictions != null && request.Client.IdentityProviderRestrictions.Any())
            {
                if (!request.Client.IdentityProviderRestrictions.Contains(currentIdp))
                {
                    _logger.LogWarning("User is logged in with idp: {0}, but idp not in client restriction list.", currentIdp);
                    return new LoginInteractionResponse() { IsLogin = true };
                }
            }

            // check if idp is local and local logins are not allowed
            if (currentIdp == Constants.BuiltInIdentityProvider)
            {
                if (_options.AuthenticationOptions.EnableLocalLogin == false ||
                    request.Client.EnableLocalLogin == false)
                {
                    _logger.LogWarning("User is logged in with local idp, but local logins not enabled.");
                    return new LoginInteractionResponse() { IsLogin = true };
                }
            }

            return new LoginInteractionResponse();
        }

        public async Task<ConsentInteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, UserConsent consent = null)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (request.PromptMode != null &&
                request.PromptMode != Constants.PromptModes.None &&
                request.PromptMode != Constants.PromptModes.Consent)
            {
                throw new ArgumentException("Invalid PromptMode");
            }

            var consentRequired = await _consent.RequiresConsentAsync(request.Client, request.Subject, request.RequestedScopes);

            if (consentRequired && request.PromptMode == Constants.PromptModes.None)
            {
                _logger.LogInformation("Prompt=none requested, but consent is required.");

                return new ConsentInteractionResponse
                {
                    Error = new AuthorizeError
                    {
                        ErrorType = ErrorTypes.Client,
                        Error = Constants.AuthorizeErrors.InteractionRequired,
                        ResponseMode = request.ResponseMode,
                        ErrorUri = request.RedirectUri,
                        State = request.State
                    }
                };
            }

            if (request.PromptMode == Constants.PromptModes.Consent || consentRequired)
            {
                var response = new ConsentInteractionResponse();

                // did user provide consent
                if (consent == null)
                {
                    // user was not yet shown conset screen
                    response.IsConsent = true;
                }
                else
                {
                    request.WasConsentShown = true;

                    // user was shown consent -- did they say yes or no
                    if (consent.Granted == false)
                    {
                        // no need to show consent screen again
                        // build access denied error to return to client
                        response.Error = new AuthorizeError
                        {
                            ErrorType = ErrorTypes.Client,
                            Error = Constants.AuthorizeErrors.AccessDenied,
                            ResponseMode = request.ResponseMode,
                            ErrorUri = request.RedirectUri,
                            State = request.State
                        };
                    }
                    else
                    {
                        // they said yes, set scopes they chose
                        request.ValidatedScopes.SetConsentedScopes(consent.ScopesConsented);

                        if (request.Client.AllowRememberConsent)
                        {
                            // remember consent
                            var scopes = Enumerable.Empty<string>();
                            if (consent.RememberConsent)
                            {
                                // remember what user actually selected
                                scopes = request.ValidatedScopes.GrantedScopes.Select(x => x.Name);
                            }

                            await _consent.UpdateConsentAsync(request.Client, request.Subject, scopes);
                        }
                    }
                }

                return response;
            }

            return new ConsentInteractionResponse();
        }
    }
}