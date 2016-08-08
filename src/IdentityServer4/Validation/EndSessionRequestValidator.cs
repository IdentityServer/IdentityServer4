// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Logging;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    internal class EndSessionRequestValidator : IEndSessionRequestValidator
    {
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly ITokenValidator _tokenValidator;
        private readonly IRedirectUriValidator _uriValidator;

        public EndSessionRequestValidator(
            ILogger<EndSessionRequestValidator> logger,
            IdentityServerContext context, 
            ITokenValidator tokenValidator, 
            IRedirectUriValidator uriValidator)
        {
            _context = context;
            _tokenValidator = tokenValidator;
            _uriValidator = uriValidator;
            _logger = logger;
        }

        public async Task<EndSessionValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            _logger.LogDebug("Start end session request validation");

            var isAuthenticated = subject != null &&
                subject.Identity != null &&
                subject.Identity.IsAuthenticated;

            if (!isAuthenticated && _context.Options.AuthenticationOptions.RequireAuthenticatedUserForSignOutMessage)
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
                if (subClaim != null && subject.Identity.IsAuthenticated)
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
    }
}