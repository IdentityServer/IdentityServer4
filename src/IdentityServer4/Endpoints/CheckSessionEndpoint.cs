// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    class CheckSessionEndpoint : IEndpoint
    {
        private readonly ILogger<CheckSessionEndpoint> _logger;
        private ISessionIdService _sessionId;

        public CheckSessionEndpoint(
            ILogger<CheckSessionEndpoint> logger,
            ISessionIdService sessionId)
        {
            _logger = logger;
            _sessionId = sessionId;
        }

        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            _logger.LogInformation("Check session iframe request");

            return Task.FromResult<IEndpointResult>(new CheckSessionResult(_sessionId.GetCookieName()));
        }
   }
}