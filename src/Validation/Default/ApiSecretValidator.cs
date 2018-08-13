﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates API secrets using the registered secret validators and parsers
    /// </summary>
    public class ApiSecretValidator : IApiSecretValidator
    {
        private readonly ILogger _logger;
        private readonly IResourceStore _resources;
        private readonly IEventService _events;
        private readonly SecretParser _parser;
        private readonly SecretValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiSecretValidator"/> class.
        /// </summary>
        /// <param name="resources">The resources.</param>
        /// <param name="parsers">The parsers.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public ApiSecretValidator(IResourceStore resources, SecretParser parsers, SecretValidator validator, IEventService events, ILogger<ApiSecretValidator> logger)
        {
            _resources = resources;
            _parser = parsers;
            _validator = validator;
            _events = events;
            _logger = logger;
        }

        /// <summary>
        /// Validates the secret on the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<ApiSecretValidationResult> ValidateAsync(HttpContext context)
        {
            _logger.LogTrace("Start API validation");

            var fail = new ApiSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(context);
            if (parsedSecret == null)
            {
                await RaiseFailureEventAsync("unknown", "No API id or secret found");

                _logger.LogError("No API secret found");
                return fail;
            }

            // load API resource
            var api = await _resources.FindApiResourceAsync(parsedSecret.Id);
            if (api == null)
            {
                await RaiseFailureEventAsync(parsedSecret.Id, "Unknown API resource");

                _logger.LogError("No API resource with that name found. aborting");
                return fail;
            }

            if (api.Enabled == false)
            {
                await RaiseFailureEventAsync(parsedSecret.Id, "API resource not enabled");

                _logger.LogError("API resource not enabled. aborting.");
                return fail;
            }

            var result = await _validator.ValidateAsync(parsedSecret, api.ApiSecrets);
            if (result.Success)
            {
                _logger.LogDebug("API resource validation success");

                var success = new ApiSecretValidationResult
                {
                    IsError = false,
                    Resource = api
                };

                await RaiseSuccessEventAsync(api.Name, parsedSecret.Type);
                return success;
            }

            await RaiseFailureEventAsync(api.Name, "Invalid API secret");
            _logger.LogError("API validation failed.");

            return fail;
        }

        private Task RaiseSuccessEventAsync(string clientId, string authMethod)
        {
            return _events.RaiseAsync(new ApiAuthenticationSuccessEvent(clientId, authMethod));
        }

        private Task RaiseFailureEventAsync(string clientId, string message)
        {
            return _events.RaiseAsync(new ApiAuthenticationFailureEvent(clientId, message));
        }
    }
}