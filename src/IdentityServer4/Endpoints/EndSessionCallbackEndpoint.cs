// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Endpoints
{
    internal class EndSessionCallbackEndpoint : IEndpointHandler
    {
        private readonly IEndSessionRequestValidator _endSessionRequestValidator;

        private readonly ILogger _logger;

        public EndSessionCallbackEndpoint(
            IEndSessionRequestValidator endSessionRequestValidator,
            ILogger<EndSessionCallbackEndpoint> logger)
        {
            _endSessionRequestValidator = endSessionRequestValidator;
            _logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for end session callback endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogDebug("Processing signout callback request");

            var parameters = context.Request.Query.AsNameValueCollection();
            var result = await _endSessionRequestValidator.ValidateCallbackAsync(parameters);

            if (result.IsError == false)
            {
                _logger.LogInformation("Successful signout callback. Client logout iframe urls: {urls}", result.FrontChannelLogoutUrls);
            }

            // todo brock: do async backchannel here

            return new EndSessionCallbackResult(result);
        }
    }
}
