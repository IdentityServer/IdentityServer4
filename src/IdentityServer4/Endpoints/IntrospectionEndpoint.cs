// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Endpoints.Results;

namespace IdentityServer4.Core.Endpoints
{
    public class IntrospectionEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly IIntrospectionResponseGenerator _generator;
        private readonly ILogger<IntrospectionEndpoint> _logger;
        private readonly IIntrospectionRequestValidator _requestValidator;
        private readonly ScopeSecretValidator _scopeSecretValidator;

        public IntrospectionEndpoint(ScopeSecretValidator scopeSecretValidator, IIntrospectionRequestValidator requestValidator, IIntrospectionResponseGenerator generator, IEventService events, ILogger<IntrospectionEndpoint> logger)
        {
            _scopeSecretValidator = scopeSecretValidator;
            _requestValidator = requestValidator;
            _generator = generator;
            _events = events;
            _logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            // validate HTTP
            if (context.HttpContext.Request.Method != "POST")
            {
                return new StatusCodeResult(405);
            }

            var scopeResult = await _scopeSecretValidator.ValidateAsync(context.HttpContext);
            if (scopeResult.Scope == null)
            {
                _logger.LogWarning("Scope unauthorized to call introspection endpoint. aborting.");
                return new StatusCodeResult(401);
            }

            var parameters = context.HttpContext.Request.Form.AsNameValueCollection();

            var validationResult = await _requestValidator.ValidateAsync(parameters, scopeResult.Scope);
            var response = await _generator.ProcessAsync(validationResult, scopeResult.Scope);

            if (validationResult.IsActive)
            {
                await RaiseSuccessEventAsync(validationResult.Token, "active", scopeResult.Scope.Name);
                return new IntrospectionResult(response);
            }

            if (validationResult.IsError)
            {
                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.MissingToken)
                {
                    _logger.LogError("Missing token");

                    await RaiseFailureEventAsync(validationResult.ErrorDescription, validationResult.Token, scopeResult.Scope.Name);
                    //todo return BadRequest("missing_token");
                    return new StatusCodeResult(400);
                }

                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.InvalidToken)
                {
                    await RaiseSuccessEventAsync(validationResult.Token, "inactive", scopeResult.Scope.Name);
                    return new IntrospectionResult(response);
                }

                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.InvalidScope)
                {
                    await RaiseFailureEventAsync("Scope not authorized to introspect token", validationResult.Token, scopeResult.Scope.Name);
                    return new IntrospectionResult(response);
                }
            }

            throw new InvalidOperationException("Invalid token introspection outcome");
        }

        private async Task RaiseSuccessEventAsync(string token, string tokenStatus, string scopeName)
        {
            await _events.RaiseSuccessfulIntrospectionEndpointEventAsync(
                token,
                tokenStatus,
                scopeName);
        }

        private async Task RaiseFailureEventAsync(string error, string token, string scopeName)
        {
            await _events.RaiseFailureIntrospectionEndpointEventAsync(
                error, token, scopeName);
        }
    }
}