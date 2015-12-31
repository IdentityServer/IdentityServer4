// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public class IdentityServerMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public IdentityServerMiddleware(RequestDelegate next, ILogger<IdentityServerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IEndpointRouter router, IdentityServerContext idSvrContext)
        {
            var endpoint = router.Find(context);
            if (endpoint != null)
            {
                var result = await endpoint.ProcessAsync(idSvrContext);

                if (result != null)
                {
                    await result.ExecuteAsync(idSvrContext);
                }

                return;
            }

            await _next(context);
        }
    }
}