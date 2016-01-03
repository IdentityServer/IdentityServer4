// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public class BaseUrlMiddleware
    {
        private readonly RequestDelegate _next;
        
        public BaseUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IdentityServerContext idsrvContext)
        {
            var request = context.Request;

            var host = request.Scheme + "://" + request.Host.Value;
            idsrvContext.SetHost(host);
            idsrvContext.SetBasePath(request.PathBase.Value.RemoveTrailingSlash());

            await _next(context);
        }
    }
}