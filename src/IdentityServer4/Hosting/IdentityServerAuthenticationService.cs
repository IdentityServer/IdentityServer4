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
                _session.CreateSessionId(principal, properties);
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
                _session.RemoveSessionIdCookie();
            }

            await _inner.SignOutAsync(context, scheme, properties);
        }

        public async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            var result = await _inner.AuthenticateAsync(context, scheme);

            var defaultScheme = await _schemes.GetDefaultSignOutSchemeAsync();
            if (scheme == null || scheme == defaultScheme.Name)
            {
                if (result?.Succeeded == true)
                {
                    _session.SetCurrentUser(result.Principal, result.Properties);
                }
            }

            return result;
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
