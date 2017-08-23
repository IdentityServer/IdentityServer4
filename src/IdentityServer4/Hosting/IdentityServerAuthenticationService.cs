using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting
{
    // todo: review
    public class IdentityServerAuthenticationService : AuthenticationService
    {
        private readonly IdentityServerOptions _options;
        private readonly IUserSession _session;
        private ILogger<IdentityServerAuthenticationService> _logger;

        public IdentityServerAuthenticationService(
            IAuthenticationSchemeProvider schemes, 
            IAuthenticationHandlerProvider handlers, 
            IClaimsTransformation transform,
            IdentityServerOptions options,
            IUserSession session,
            ILogger<IdentityServerAuthenticationService> logger)
            : base(schemes, handlers, transform)
        {
            _options = options;
            _session = session;
            _logger = logger;
        }

        public override async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            var defaultScheme = await Schemes.GetDefaultSignInSchemeAsync();

            if (scheme == null || scheme == defaultScheme.Name)
            {
                AugmentPrincipal(principal);
            }

            if (properties == null) properties = new AuthenticationProperties();
            _session.CreateSessionId(properties);

            await base.SignInAsync(context, scheme, principal, properties);
        }

        private void AugmentPrincipal(ClaimsPrincipal principal)
        {
            _logger.LogDebug("Augmenting SignInContext");

            principal.AssertRequiredClaims();
            principal.AugmentMissingClaims(_options.UtcNow);
        }

        public override async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            var defaultScheme = await Schemes.GetDefaultSignOutSchemeAsync();

            if (scheme == null || scheme == defaultScheme.Name)
            {
                _session.RemoveSessionIdCookie();
            }

            await base.SignOutAsync(context, scheme, properties);
        }
    }
}
