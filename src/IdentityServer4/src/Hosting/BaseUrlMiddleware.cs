// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using IdentityServer4.Configuration;

#pragma warning disable 1591

namespace IdentityServer4.Hosting
{
    public class BaseUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;

        public BaseUrlMiddleware(RequestDelegate next, IdentityServerOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            Handle(context, _options);

            await _next(context);
        }

        internal static void Handle(HttpContext context, IdentityServerOptions options)
        {
            var request = context.Request;

            if (options.PublicOrigin.IsPresent())
            {
                context.SetIdentityServerOrigin(options.PublicOrigin);
            }

            context.SetIdentityServerBasePath(request.PathBase.Value.RemoveTrailingSlash());

        }
    }
}