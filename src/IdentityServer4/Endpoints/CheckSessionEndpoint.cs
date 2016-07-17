// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    class CheckSessionEndpoint : IEndpoint
    {
        private readonly ILogger<CheckSessionEndpoint> _logger;
        private readonly SessionCookie _sessionCookie;

        public CheckSessionEndpoint(
            ILogger<CheckSessionEndpoint> logger,
            SessionCookie sessionCookie)
        {
            _logger = logger;
            _sessionCookie = sessionCookie;
        }

        public Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            _logger.LogInformation("Check session iframe request");

            return Task.FromResult<IEndpointResult>(new CheckSessionResult(_sessionCookie.GetCookieName()));
        }
   }
}