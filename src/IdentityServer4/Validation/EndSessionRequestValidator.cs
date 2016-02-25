using IdentityServer4.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Validation
{
    public class EndSessionRequestValidator : IEndSessionRequestValidator
    {
        private readonly IdentityServerContext _context;
        private readonly ILogger<EndSessionRequestValidator> _logger;
        private readonly ITokenValidator _tokenValidator;
        private readonly IRedirectUriValidator _uriValidator;

        public EndSessionRequestValidator(
            IdentityServerContext context,
            ILogger<EndSessionRequestValidator> logger, 
            ITokenValidator tokenValidator, 
            IRedirectUriValidator uriValidator)
        {
            _context = context;
            _logger = logger;
            _tokenValidator = tokenValidator;
            _uriValidator = uriValidator;
        }

        public async Task<EndSessionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            _logger.LogInformation("Start end session request validation");

            if (parameters.Count == 0)
            {
                return Invalid("Invalid request");
            }

            if (!subject.Identity.IsAuthenticated && _context.Options.AuthenticationOptions.RequireAuthenticatedUserForSignOutMessage)
            {
                _logger.LogWarning("User is anonymous. Ignoring end session parameters");

                return Invalid("User is anonymous");
            }

            var validationResult = new EndSessionRequestValidationResult
            {
                IsError = false
            };

            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);

            if (idTokenHint.IsMissing())
            {
                _logger.LogInformation("Id token hint not found");
                return Invalid("Id token hint not found");
            }

            // validate id_token - no need to validate token life time
            var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint, null, false);
            if (tokenValidationResult.IsError)
            {
                _logger.LogInformation("Invalid id token hint");
                return Invalid("Invalid id token hint");
            }

            validationResult.Client = tokenValidationResult.Client;

            var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == IdentityModel.JwtClaimTypes.Subject);
            if (subClaim != null && subject.Identity.IsAuthenticated)
            {
                if (subject.GetSubjectId() != subClaim.Value)
                {
                    _logger.LogInformation("Current user does not match identity token");
                    return Invalid("Current user does not match identity token");
                }
            }

            var redirectUri = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
            
            validationResult.PostLogoutUri = redirectUri;

            if (await _uriValidator.IsPostLogoutRedirectUriValidAsync(redirectUri, validationResult.Client) == false)
            {
                _logger.LogInformation("Invalid post logout URI for this client");
                return Invalid("Invalid post logout URI for this client");
            }

            var state = parameters.Get(Constants.EndSessionRequest.State);
            
            validationResult.State = state;

            _logger.LogInformation("End end session request validation");

            return validationResult;
        }

        private EndSessionRequestValidationResult Invalid(string errorMessage)
        {
            return new EndSessionRequestValidationResult
            {
                IsError = true,
                Error = errorMessage
            };
        }
    }
}
