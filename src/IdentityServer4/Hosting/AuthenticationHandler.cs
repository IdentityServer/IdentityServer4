// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using System.Collections.Generic;

namespace IdentityServer4.Hosting
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly IdentityServerOptions _options;
        private readonly HttpContext _context;
        private IAuthenticationHandler _handler;
        private readonly ISessionIdService _sessionId;
        private readonly IEnumerable<ISignInValdationService> _signInValidators;

        public AuthenticationHandler(IHttpContextAccessor context, IdentityServerOptions options, ISessionIdService sessionId, IEnumerable<ISignInValdationService> signInValidators)
        {
            _context = context.HttpContext;
            _options = options;
            _sessionId = sessionId;
            _signInValidators = signInValidators;
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
            if (context.AuthenticationScheme == _options.AuthenticationOptions.EffectiveAuthenticationScheme)
            {
                await AugmentContextAsync(context);
            }
            await _handler.SignInAsync(context);
        }

        private async Task AugmentContextAsync(SignInContext context)
        {
            await RunValidatorsAsync(context);

            context.Principal.AssertRequiredClaims();
            context.Principal.AugmentMissingClaims();

            await _sessionId.AddSessionIdAsync(context);
        }

        async Task RunValidatorsAsync(SignInContext context)
        {
            foreach(var validator in _signInValidators)
            {
                await validator.ValidateAsync(context);
            }
        }
 
        public Task SignOutAsync(SignOutContext context)
        {
            return _handler.SignOutAsync(context);
        }

        internal void Init()
        {
            var auth = GetAuthentication();
            _handler = auth.Handler;
            auth.Handler = this;
        }

        internal void Cleanup()
        {
            var auth = GetAuthentication();
            auth.Handler = _handler;
        }

        IHttpAuthenticationFeature GetAuthentication()
        {
            var auth = _context.Features.Get<IHttpAuthenticationFeature>();
            if (auth == null)
            {
                auth = new HttpAuthenticationFeature();
                _context.Features.Set(auth);
            }
            return auth;
        }
    }
}
