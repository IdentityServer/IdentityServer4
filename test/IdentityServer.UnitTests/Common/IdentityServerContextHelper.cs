// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Common
{
    public class IdentityServerContextHelper
    {
        public static IdentityServerContext Create(HttpContext context = null, IdentityServerOptions options = null)
        {
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = context ?? new DefaultHttpContext();
            return new IdentityServerContext(accessor, options ?? TestIdentityServerOptions.Create());
        }
    }
}