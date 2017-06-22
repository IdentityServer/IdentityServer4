// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Models;

namespace IdentityServer4.UnitTests.Common
{
    class MockHttpContextAccessor : IHttpContextAccessor
    {
        HttpContext _context = new DefaultHttpContext();

        public MockHttpContextAccessor(
            IdentityServerOptions options = null,
            IUserSession userSession = null,
            IMessageStore<EndSession> endSessionStore = null)
        {
            options = options ?? TestIdentityServerOptions.Create();

            var services = new ServiceCollection();
            services.AddSingleton(options);
            if (userSession == null)
            {
                services.AddTransient<IUserSession, DefaultUserSession>();
            }
            else
            {
                services.AddSingleton(userSession);
            }

            if (endSessionStore == null)
            {
                services.AddTransient<IMessageStore<EndSession>, ProtectedDataMessageStore<EndSession>>();
            }
            else
            {
                services.AddSingleton(endSessionStore);
            }

            _context.RequestServices = services.BuildServiceProvider();

            // setups the authN middleware feature
            //_context.SetUser(null);
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
