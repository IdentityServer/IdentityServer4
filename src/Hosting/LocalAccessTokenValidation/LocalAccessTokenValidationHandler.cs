using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IdentityServer4.Hosting.LocalAccessTokenValidation
{
    /// <summary>
    /// Authentication handler for validating access token via ITokenValidator
    /// </summary>
    public class LocalAccessTokenValidationHandler : AuthenticationHandler<LocalAccessTokenValidationOptions>
    {
        private readonly ITokenValidator _tokenValidator;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public LocalAccessTokenValidationHandler(IOptionsMonitor<LocalAccessTokenValidationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ITokenValidator tokenValidator)
            : base(options, logger, encoder, clock)
        {
            _tokenValidator = tokenValidator;
            _logger = logger.CreateLogger<LocalAccessTokenValidationHandler>();
        }

        /// <summary>
        /// Tries to validate a token on the current request
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            _logger.LogTrace("HandleAuthenticateAsync called");

            string token = null;

            string authorization = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                return AuthenticateResult.Fail("No Authorization Header is sent.");
            }

            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("No Access Token is sent.");
            }

            _logger.LogTrace("Token found: {token}", token);

            TokenValidationResult result = await _tokenValidator.ValidateAccessTokenAsync(token, Options.ExpectedScope);

            if (result.IsError)
            {
                _logger.LogTrace("Failed to validate the token");

                return AuthenticateResult.Fail(result.Error);
            }

            _logger.LogTrace("Successfully validated the token.");

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(result.Claims, Scheme.Name, Options.NameClaimType, Options.RoleClaimType);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            AuthenticationProperties authenticationProperties = new AuthenticationProperties();

            if (Options.SaveToken)
            {
                authenticationProperties.StoreTokens(new[]
                {
                    new AuthenticationToken { Name = "access_token", Value = token }
                });
            }

            AuthenticationTicket authenticationTicket = new AuthenticationTicket(claimsPrincipal, authenticationProperties, Scheme.Name);
            return AuthenticateResult.Success(authenticationTicket);
        }
    }
}