// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// The discovery endpoint
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpoint" />
    public class DiscoveryEndpoint : IEndpoint
    {
        private readonly IdentityServerOptions _options;
        private readonly IDiscoveryResponseGenerator _responseGenerator;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryEndpoint" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="logger">The logger.</param>
        public DiscoveryEndpoint(
            IdentityServerOptions options,
            IDiscoveryResponseGenerator responseGenerator,
            ILogger<DiscoveryEndpoint> logger)
        {
            _logger = logger;
            _options = options;
            _responseGenerator = responseGenerator;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing discovery request.");

            // validate HTTP
            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Discovery endpoint only supports GET requests");
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            if (context.Request.Path.Value.EndsWith("/jwks"))
            {
                return ExecuteJwksAsync();
            }
            else
            {
                return ExecuteDiscoDocAsync(context);
            }
        }

        private async Task<IEndpointResult> ExecuteDiscoDocAsync(HttpContext context)
        {
            _logger.LogDebug("Start discovery request");

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
            {
                _logger.LogInformation("Discovery endpoint disabled. 404.");
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            var baseUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash();
            var issuerUri = context.GetIdentityServerIssuerUri();

            // generate response
            _logger.LogTrace("Calling into discovery response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri);

            return new DiscoveryDocumentResult(response, _options.Discovery.ResponseCacheInterval);
        }

        private async Task<IEndpointResult> ExecuteJwksAsync()
        {
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