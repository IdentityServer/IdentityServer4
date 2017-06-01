// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

#pragma warning disable 1591

namespace IdentityServer4.Hosting
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly IdentityServerOptions _options;
        private readonly IHttpContextAccessor _context;
        private IAuthenticationHandler _handler;
        private readonly ISessionIdService _sessionId;
        private readonly ILogger _logger;
        private readonly IEventService _events;

        public AuthenticationHandler(
            IHttpContextAccessor context, 
            IdentityServerOptions options, 
            ISessionIdService sessionId,
            IEventService events,
            ILogger<AuthenticationHandler> logger)
        {
            _context = context;
            _options = options;
            _sessionId = sessionId;
            _events = events;
            _logger = logger;
        }

        public Task AuthenticateAsync(AuthenticateContext context)
        {
            return _handler.AuthenticateAsync(context);
        }

        public Task ChallengeAsync(ChallengeContext context)
        {
            return _handler.ChallengeAsync(context);
        }

        public void GetDescriptions(DescribeSchemesContext context)
        {
            _handler.GetDescriptions(context);
        }

        public async Task SignInAsync(SignInContext context)
        {
            if (context.AuthenticationScheme == _options.Authentication.EffectiveAuthenticationScheme)
            {
                AugmentContext(context);
            }
            await _handler.SignInAsync(context);
        }

        private void AugmentContext(SignInContext context)
        {
            _logger.LogDebug("Augmenting SignInContext");

            context.Principal.AssertRequiredClaims();
            context.Principal.AugmentMissingClaims();

            _sessionId.CreateSessionId(context);
        }

        public async Task SignOutAsync(SignOutContext context)
        {
            if (context.AuthenticationScheme == _options.Authentication.EffectiveAuthenticationScheme)
            {
                _sessionId.RemoveCookie();
            }
            await _handler.SignOutAsync(context);
        }

        internal async Task InitAsync()
        {
            var auth = GetAuthentication();
            _handler = auth.Handler;
            auth.Handler = this;

            await _sessionId.EnsureSessionCookieAsync();
        }

        internal void Cleanup()
        {
            var auth = GetAuthentication();
            auth.Handler = _handler;
        }

        IHttpAuthenticationFeature GetAuthentication()
        {
            var auth = _context.HttpContext.Features.Get<IHttpAuthenticationFeature>();
            if (auth == null)
            {
                auth = new HttpAuthenticationFeature();
                _context.HttpContext.Features.Set(auth);
            }
            return auth;
        }
    }
}
