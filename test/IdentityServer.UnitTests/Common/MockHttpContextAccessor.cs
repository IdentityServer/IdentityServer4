// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Services;
using IdentityServer4.Services.Default;

namespace IdentityServer4.UnitTests.Common
{
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        HttpContext _context = new DefaultHttpContext();

        public MockHttpContextAccessor(IdentityServerOptions options = null)
        {
            options = options ?? TestIdentityServerOptions.Create();

            var services = new ServiceCollection();
            services.AddSingleton(options);
            services.AddTransient<ISessionIdService, DefaultSessionIdService>();

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
