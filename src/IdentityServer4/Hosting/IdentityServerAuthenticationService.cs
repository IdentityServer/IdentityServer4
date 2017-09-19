﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Configuration.DependencyInjection;
using IdentityServer4.Extensions;
using System;

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
        private readonly ISystemClock _clock;
        private readonly IUserSession _session;
        private ILogger<IdentityServerAuthenticationService> _logger;

        public IdentityServerAuthenticationService(
            Decorator<IAuthenticationService> decorator,
            IAuthenticationSchemeProvider schemes,
            IAuthenticationHandlerProvider handlers,
            IClaimsTransformation transform,
            ISystemClock clock,
            IUserSession session,
            ILogger<IdentityServerAuthenticationService> logger)
        {
            _inner = decorator.Instance;
            _schemes = schemes;
            _clock = clock;
            _session = session;
            _logger = logger;
        }

        async Task<string> GetCookieAuthenticationSchemeAsync()
        {
            var scheme = await _schemes.GetDefaultAuthenticateSchemeAsync();
            if (scheme == null)
            {
                throw new InvalidOperationException($"No DefaultAuthenticateScheme found.");
            }
            return scheme.Name;
        }

        public async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            var defaultScheme = await _schemes.GetDefaultSignInSchemeAsync();
            var cookieScheme = await GetCookieAuthenticationSchemeAsync();

            if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
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
            principal.AugmentMissingClaims(_clock.UtcNow.UtcDateTime);
        }

        public async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            var defaultScheme = await _schemes.GetDefaultSignOutSchemeAsync();
            var cookieScheme = await GetCookieAuthenticationSchemeAsync();

            if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
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
