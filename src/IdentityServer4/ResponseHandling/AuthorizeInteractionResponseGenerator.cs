// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling
{
    class AuthorizeInteractionResponseGenerator : IAuthorizeInteractionResponseGenerator
    {
        private readonly ILogger<AuthorizeInteractionResponseGenerator> _logger;
        private readonly IConsentService _consent;
        private readonly IProfileService _profile;

        public AuthorizeInteractionResponseGenerator(
            ILogger<AuthorizeInteractionResponseGenerator> logger, 
            IdentityServerOptions options, 
            IConsentService consent, 
            IProfileService profile)
        {
            _logger = logger;
            _consent = consent;
            _profile = profile;
        }

        public async Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
        {
            var result = await ProcessLoginAsync(request);
            if (result.IsLogin || result.IsError)
            {
                return result;
            }

            return await ProcessConsentAsync(request, consent);
        }

        internal async Task<InteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request)
        {
            if (request.PromptMode == OidcConstants.PromptModes.Login)
            {
                // remove prompt so when we redirect back in from login page
                // we won't think we need to force a prompt again
                request.RemovePrompt();

                _logger.LogInformation("Redirecting to login page because of prompt=login");

                return new InteractionResponse() { IsLogin = true };
            }

            // unauthenticated user
            var isAuthenticated = request.Subject.Identity.IsAuthenticated;
            
            // user de-activated
            bool isActive = false;

            if (isAuthenticated)
            {
                var isActiveCtx = new IsActiveContext(request.Subject, request.Client);
                await _profile.IsActiveAsync(isActiveCtx);
                
                isActive = isActiveCtx.IsActive;
            }

            if (!isAuthenticated || !isActive)
            {
                if (!isAuthenticated) _logger.LogInformation("User is not authenticated.");
                else if (!isActive) _logger.LogInformation("User is not active.");

                // prompt=none means user must be signed in already
                if (request.PromptMode == OidcConstants.PromptModes.None)
                {
                    _logger.LogInformation("prompt=none was requested but user is not authenticated/active.");

                    return new InteractionResponse
                    {
                        Error = OidcConstants.AuthorizeErrors.LoginRequired,
                    };
                }

                return new InteractionResponse() { IsLogin = true };
            }

            // check current idp
            var currentIdp = request.Subject.GetIdentityProvider();

            // check if idp login hint matches current provider
            var idp = request.GetIdP();
            if (idp.IsPresent())
            {
                if (idp != currentIdp)
                {
                    _logger.LogInformation("Current IdP is not the requested IdP. Redirecting to login");
                    _logger.LogInformation("Current: {0} -- Requested: {1}", currentIdp, idp);

                    return new InteractionResponse() { IsLogin = true };
                }
            }

            // check authentication freshness
            if (request.MaxAge.HasValue)
            {
                var authTime = request.Subject.GetAuthenticationTime();
                if (DateTimeHelper.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    _logger.LogInformation("Requested MaxAge exceeded.");

                    return new InteractionResponse() { IsLogin = true };
                }
            }

            // check local idp restrictions
            if (currentIdp == Constants.LocalIdentityProvider && !request.Client.EnableLocalLogin)
            {
                _logger.LogInformation("User logged in locally, but client does not allow local logins");
                return new InteractionResponse() { IsLogin = true };
            }

            // check external idp restrictions
            if (request.Client.IdentityProviderRestrictions != null && request.Client.IdentityProviderRestrictions.Any())
            {
                if (!request.Client.IdentityProviderRestrictions.Contains(currentIdp))
                {
                    _logger.LogWarning("User is logged in with idp: {0}, but idp not in client restriction list.", currentIdp);
                    return new InteractionResponse() { IsLogin = true };
                }
            }

            return new InteractionResponse();
        }

        internal async Task<InteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (request.PromptMode != null &&
                request.PromptMode != OidcConstants.PromptModes.None &&
                request.PromptMode != OidcConstants.PromptModes.Consent)
            {
                throw new ArgumentException("Invalid PromptMode");
            }

            var consentRequired = await _consent.RequiresConsentAsync(request.Subject, request.Client, request.RequestedScopes);

            if (consentRequired && request.PromptMode == OidcConstants.PromptModes.None)
            {
                _logger.LogInformation("Prompt=none requested, but consent is required.");

                return new InteractionResponse
                {
                    Error = OidcConstants.AuthorizeErrors.ConsentRequired,
                };
            }

            if (request.PromptMode == OidcConstants.PromptModes.Consent || consentRequired)
            {
                var response = new InteractionResponse();

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
                        response.Error = OidcConstants.AuthorizeErrors.AccessDenied;
                    }
                    else
                    {
                        // double check that required scopes are in the list of consented scopes
                        var valid = request.ValidatedScopes.ValidateRequiredScopes(consent.ScopesConsented);
                        if (valid == false)
                        {
                            response.Error = OidcConstants.AuthorizeErrors.AccessDenied;
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

                                await _consent.UpdateConsentAsync(request.Subject, request.Client, scopes);
                            }
                        }
                    }
                }

                return response;
            }

            return new InteractionResponse();
        }
    }
}