// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Services;

namespace IdentityServer4.UnitTests.Common
{
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        HttpContext _context = new DefaultHttpContext();

        public MockHttpContextAccessor(IdentityServerOptions options = null, 
            ISessionIdService sessionIdService = null,
            IClientSessionService clientSessionService = null)
        {
            options = options ?? TestIdentityServerOptions.Create();

            var services = new ServiceCollection();
            services.AddSingleton(options);
            if (sessionIdService == null)
            {
                services.AddTransient<ISessionIdService, DefaultSessionIdService>();
            }
            else
            {
                services.AddSingleton(sessionIdService);
            }

            if (clientSessionService == null)
            {
                services.AddTransient<IClientSessionService, DefaultClientSessionService>();
            }
            else
            {
                services.AddSingleton(clientSessionService);
            }

            _context.RequestServices = services.BuildServiceProvider();

            // setups the authN middleware feature
            _context.SetUser(null);
        }

        public HttpContext HttpContext
        {
            get
            {
                return _context;
            }

            set
            {
                _context = value;
            }
        }
    }
}
