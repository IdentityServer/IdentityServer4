// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Specialized;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Endpoints
{
    class EndSessionEndpoint : IEndpoint
    {
        private readonly IEndSessionRequestValidator _endSessionRequestValidator;
        private readonly ILogger<EndSessionEndpoint> _logger;

        public EndSessionEndpoint(IEndSessionRequestValidator endSessionRequestValidator, ILogger<EndSessionEndpoint> logger)
        {
            _endSessionRequestValidator = endSessionRequestValidator;
            _logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Path == Constants.ProtocolRoutePaths.EndSession.EnsureLeadingSlash())
            {
                return await ProcessSignoutAsync(context);
            }

            if (context.Request.Path == Constants.ProtocolRoutePaths.EndSessionCallback.EnsureLeadingSlash())
            {
                return await ProcessSignoutCallbackAsync(context);
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        private async Task<IEndpointResult> ProcessSignoutAsync(HttpContext context)
        {
            _logger.LogInformation("Processing signout request");

            NameValueCollection parameters = null;
            if (context.Request.Method == "GET")
            {
                parameters = context.Request.Query.AsNameValueCollection();
            }
            else if (context.Request.Method == "POST")
            {
                parameters = context.Request.Form.AsNameValueCollection();
            }
            else
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await context.GetIdentityServerUserAsync();
            var result = await _endSessionRequestValidator.ValidateAsync(parameters, user);

            return new EndSessionResult(result);
        }

        private async Task<IEndpointResult> ProcessSignoutCallbackAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogInformation("Processing singout callback request");

            var parameters = context.Request.Query.AsNameValueCollection();
            var result = await _endSessionRequestValidator.ValidateCallbackAsync(parameters);

            return new EndSessionCallbackResult(result);
        }
    }
}