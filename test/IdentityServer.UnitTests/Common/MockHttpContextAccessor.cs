// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.Common;
using IdentityServer4.Hosting;

namespace IdentityServer.UnitTests.Common
{
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        HttpContext _context = new DefaultHttpContext();

        public MockHttpContextAccessor(IdentityServerOptions options = null)
        {
            options = options ?? TestIdentityServerOptions.Create();

            var services = new ServiceCollection();
            services.AddSingleton(options);
            services.AddTransient<SessionCookie>();

            _context.RequestServices = services.BuildServiceProvider();
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
