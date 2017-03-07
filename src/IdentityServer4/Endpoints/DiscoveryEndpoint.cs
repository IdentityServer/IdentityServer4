﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
    public class DiscoveryEndpoint : IEndpoint
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IDiscoveryResponseGenerator _responseGenerator;

        public DiscoveryEndpoint(
            ILogger<DiscoveryEndpoint> logger, 
            IdentityServerOptions options,
            IDiscoveryResponseGenerator responseGenerator)
        {
            _logger = logger;
            _options = options;
            _responseGenerator = responseGenerator;
        }

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

            var response = await _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri);
            return new DiscoveryDocumentResult(response);
        }

        private async Task<IEndpointResult> ExecuteJwksAsync()
        {
            _logger.LogDebug("Start key discovery request");

            if (_options.Discovery.ShowKeySet == false)
            {
                _logger.LogInformation("Key discovery disabled. 404.");
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            var response = await _responseGenerator.CreateJwkDocumentAsync();
            return new JsonWebKeysResult(response);
        }
    }
}