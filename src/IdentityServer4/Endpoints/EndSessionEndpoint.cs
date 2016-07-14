// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Http;
using System.Collections.Specialized;

namespace IdentityServer4.Endpoints
{
    class EndSessionEndpoint : IEndpoint
    {
        private readonly ILogger<EndSessionEndpoint> _logger;
        private readonly ClientListCookie _clientListCookie;

        public EndSessionEndpoint(
            ILogger<EndSessionEndpoint> logger, 
            ClientListCookie clientListCookie)
        {
            _logger = logger;
            _clientListCookie = clientListCookie;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.EndSession.EnsureLeadingSlash())
            {
                return await ProcessSignoutAsync(context);
            }

            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.EndSessionCallback.EnsureLeadingSlash())
            {
                return await ProcessSignoutCallbackAsync(context);
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        private Task<IEndpointResult> ProcessSignoutAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Processing singout request");

            NameValueCollection parameters = null;
            if (context.HttpContext.Request.Method == "GET")
            {
                parameters = context.HttpContext.Request.Query.AsNameValueCollection();
            }
            else if (context.HttpContext.Request.Method == "POST")
            {
                parameters = context.HttpContext.Request.Form.AsNameValueCollection();
            }
            else
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }




            return Task.FromResult<IEndpointResult>(new LogoutPageResult());
        }

        private Task<IEndpointResult> ProcessSignoutCallbackAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            _logger.LogInformation("Processing singout callback request");

            return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.NoContent));
        }
    }
}