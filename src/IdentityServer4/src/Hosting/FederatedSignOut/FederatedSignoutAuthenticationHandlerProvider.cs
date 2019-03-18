﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Configuration.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Hosting.FederatedSignOut
{
    // this intercepts IAuthenticationRequestHandler authentication handlers
    // to detect when they are handling federated signout. when they are invoked,
    // call signout on the default authentication scheme, and return 200 then 
    // we assume they are handling the federated signout in an iframe. 
    // based on this assumption, we then render our federated signout iframes 
    // to any current clients.
    internal class FederatedSignoutAuthenticationHandlerProvider : IAuthenticationHandlerProvider
    {
        private readonly IAuthenticationHandlerProvider _provider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FederatedSignoutAuthenticationHandlerProvider(
            Decorator<IAuthenticationHandlerProvider> decorator, 
            IHttpContextAccessor httpContextAccessor)
        {
            _provider = decorator.Instance;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IAuthenticationHandler> GetHandlerAsync(HttpContext context, string authenticationScheme)
        {
            var handler = await _provider.GetHandlerAsync(context, authenticationScheme);
            if (handler is IAuthenticationRequestHandler requestHandler)
            {
                if (requestHandler is IAuthenticationSignInHandler signinHandler)
                {
                    return new AuthenticationRequestSignInHandlerWrapper(signinHandler, _httpContextAccessor);
                }

                if (requestHandler is IAuthenticationSignOutHandler signoutHandler)
                {
                    return new AuthenticationRequestSignOutHandlerWrapper(signoutHandler, _httpContextAccessor);
                }

                return new AuthenticationRequestHandlerWrapper(requestHandler, _httpContextAccessor);
            }

            return handler;
        }
    }
}
