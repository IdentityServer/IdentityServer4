// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Net;
using Bornlogic.IdentityServer.Endpoints.Results;
using Bornlogic.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Endpoints
{
    internal class CheckSessionEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;

        public CheckSessionEndpoint(ILogger<CheckSessionEndpoint> logger)
        {
            _logger = logger;
        }

        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            IEndpointResult result;

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Invalid HTTP method for check session endpoint");
                result = new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
            else
            {
                _logger.LogDebug("Rendering check session result");
                result = new CheckSessionResult();
            }

            return Task.FromResult(result);
        }
   }
}