using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Configuration.DependencyInjection;
using IdentityServer4.Extensions;

namespace IdentityServer4.Hosting
{
    // todo: review
    // this decorates the real authentication service to detect when the 
    // user is being signed in. this allows us to ensure the user has
    // the claims needed for identity server to do its job. it also allows
    // us to track signin/signout so we can issue/remove the session id
    // cookie used for check session iframe for session management spec.
    // finally, we track if signout is called to collaborate with the 
    // FederatedSignoutAuthenticationHandlerProvider for federated signout.
    class IdentityServerAuthenticationService : IAuthenticationService
    {
        private IAuthenticationService _inner;
        private IAuthenticationSchemeProvider _schemes;
        private readonly IdentityServerOptions _options;
        private readonly IUserSession _session;
        private ILogger<IdentityServerAuthenticationService> _logger;

        public IdentityServerAuthenticationService(
            Decorator<IAuthenticationService> decorator,
            IAuthenticationSchemeProvider schemes,
            IAuthenticationHandlerProvider handlers,
            IClaimsTransformation transform,
            IdentityServerOptions options,
            IUserSession session,
            ILogger<IdentityServerAuthenticationService> logger)
        {
            _inner = decorator.Instance;
            _schemes = schemes;
            _options = options;
            _session = session;
            _logger = logger;
        }

        public async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            var defaultScheme = await _schemes.GetDefaultSignInSchemeAsync();
            if (scheme == null || scheme == defaultScheme.Name)
            {
                AugmentPrincipal(principal);

                if (properties == null) properties = new AuthenticationProperties();
                await _session.CreateSessionIdAsync(principal, properties);
            }

            await _inner.SignInAsync(context, scheme, principal, properties);
        }

        private void AugmentPrincipal(ClaimsPrincipal principal)
        {
            _logger.LogDebug("Augmenting SignInContext");

            principal.AssertRequiredClaims();
            principal.AugmentMissingClaims(_options.UtcNow);
        }

        public async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            var defaultScheme = await _schemes.GetDefaultSignOutSchemeAsync();
            if (scheme == null || scheme == defaultScheme.Name)
            {
                context.SetSignOutCalled();
                await _session.RemoveSessionIdCookieAsync();
            }

            await _inner.SignOutAsync(context, scheme, properties);
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            return _inner.AuthenticateAsync(context, scheme);
        }

        public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return _inner.ChallengeAsync(context, scheme, properties);
        }

        public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return _inner.ForbidAsync(context, scheme, properties);
        }
    }
}
