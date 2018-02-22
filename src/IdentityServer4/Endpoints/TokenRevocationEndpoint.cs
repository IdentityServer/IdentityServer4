﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Events;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Extensions;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// The revocation endpoint
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpointHandler" />
    internal class TokenRevocationEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;
        private readonly IClientSecretValidator _clientValidator;
        private readonly ITokenRevocationRequestValidator _requestValidator;
        private readonly ITokenRevocationResponseGenerator _responseGenerator;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRevocationEndpoint" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="clientValidator">The client validator.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="events">The events.</param>
        public TokenRevocationEndpoint(ILogger<TokenRevocationEndpoint> logger,
            IClientSecretValidator clientValidator,
            ITokenRevocationRequestValidator requestValidator,
            ITokenRevocationResponseGenerator responseGenerator,
            IEventService events)
        {
            _logger = logger;
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _responseGenerator = responseGenerator;

            _events = events;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing revocation request.");

            if (!HttpMethods.IsPost(context.Request.Method))
            {
                _logger.LogWarning("Invalid HTTP method");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (!context.Request.HasFormContentType)
            {
                _logger.LogWarning("Invalid media type");
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            }

            var response = await ProcessRevocationRequestAsync(context);

            return response;
        }

        private async Task<IEndpointResult> ProcessRevocationRequestAsync(HttpContext context)
        {
            _logger.LogDebug("Start revocation request.");

            // validate client
            var clientValidationResult = await _clientValidator.ValidateAsync(context);

            if (clientValidationResult.IsError)
            {
                return new TokenRevocationErrorResult(OidcConstants.TokenErrors.InvalidClient);
            }

            _logger.LogTrace("Client validation successful");

            // validate the token request
            var form = (await context.Request.ReadFormAsync()).AsNameValueCollection();

            _logger.LogTrace("Calling into token revocation request validator: {type}", _requestValidator.GetType().FullName);
            var requestValidationResult = await _requestValidator.ValidateRequestAsync(form, clientValidationResult.Client);

            if (requestValidationResult.IsError)
            {
                return new TokenRevocationErrorResult(requestValidationResult.Error);
            }

            _logger.LogTrace("Calling into token revocation response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.ProcessAsync(requestValidationResult);

            if (response.Success)
            {
                _logger.LogInformation("Token successfully revoked");
                await _events.RaiseAsync(new TokenRevokedSuccessEvent(requestValidationResult, requestValidationResult.Client));
            }
            else
            {
                _logger.LogInformation("No matching token found");
            }

            if (response.Error.IsPresent()) return new TokenRevocationErrorResult(response.Error);

            return new StatusCodeResult(HttpStatusCode.OK);
        }
    }
}