// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Endpoints
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
            this._logger = logger;
            this._options = options;
            this._responseGenerator = responseGenerator;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            this._logger.LogTrace("Processing discovery request.");

            // validate HTTP
            if (context.Request.Method != "GET")
            {
                this._logger.LogWarning("Discovery endpoint only supports GET requests");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            this._logger.LogDebug("Start key discovery request");

            if (this._options.Discovery.ShowKeySet == false)
            {
                this._logger.LogInformation("Key discovery disabled. 404.");
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            // generate response
            this._logger.LogTrace("Calling into discovery response generator: {type}", this._responseGenerator.GetType().FullName);
            var response = await this._responseGenerator.CreateJwkDocumentAsync();

            return new JsonWebKeysResult(response, this._options.Discovery.ResponseCacheInterval);
        }
    }
}
