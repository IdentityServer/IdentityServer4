// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Endpoints.Results;
using Bornlogic.IdentityServer.Hosting;
using Bornlogic.IdentityServer.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Endpoints
{
    internal class DiscoveryKeyEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;

        private readonly IdentityServerOptions _options;

        private readonly IDiscoveryResponseGenerator _responseGenerator;

        public DiscoveryKeyEndpoint(
            IdentityServerOptions options,
            IDiscoveryResponseGenerator responseGenerator,
            ILogger<DiscoveryKeyEndpoint> logger)
        {
            _logger = logger;
            _options = options;
            _responseGenerator = responseGenerator;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing discovery request.");

            // validate HTTP
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Discovery endpoint only supports GET requests");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogDebug("Start key discovery request");

            if (_options.Discovery.ShowKeySet == false)
            {
                _logger.LogInformation("Key discovery disabled. 404.");
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            // generate response
            _logger.LogTrace("Calling into discovery response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.CreateJwkDocumentAsync();

            return new JsonWebKeysResult(response, _options.Discovery.ResponseCacheInterval);
        }
    }
}
