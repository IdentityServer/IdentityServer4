// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Events;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Services;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public class ScopeSecretValidator
    {
        private readonly ILogger _logger;
        private readonly IScopeStore _scopes;
        private readonly IEventService _events;
        private readonly SecretParser _parser;
        private readonly SecretValidator _validator;

        public ScopeSecretValidator(IScopeStore scopes, SecretParser parsers, SecretValidator validator, IEventService events, ILogger<ScopeSecretValidator> logger)
        {
            _scopes = scopes;
            _parser = parsers;
            _validator = validator;
            _events = events;
            _logger = logger;
        }

        public async Task<ScopeSecretValidationResult> ValidateAsync(HttpContext context)
        {
            _logger.LogVerbose("Start scope validation");

            var fail = new ScopeSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(context);
            if (parsedSecret == null)
            {
                await RaiseFailureEvent("unknown", "No scope id or secret found");

                _logger.LogInformation("No scope secret found");
                return fail;
            }

            // load scope
            var scope = (await _scopes.FindScopesAsync(new[] { parsedSecret.Id })).FirstOrDefault();
            if (scope == null)
            {
                await RaiseFailureEvent(parsedSecret.Id, "Unknown scope");

                _logger.LogInformation("No scope with that name found. aborting");
                return fail;
            }

            var result = await _validator.ValidateAsync(parsedSecret, scope.ScopeSecrets);
            if (result.Success)
            {
                _logger.LogInformation("Scope validation success");

                var success = new ScopeSecretValidationResult
                {
                    IsError = false,
                    Scope = scope
                };

                await RaiseSuccessEvent(scope.Name);
                return success;
            }

            await RaiseFailureEvent(scope.Name, "Invalid client secret");
            _logger.LogInformation("Scope validation failed.");

            return fail;
        }

        private async Task RaiseSuccessEvent(string clientId)
        {
            await _events.RaiseSuccessfulClientAuthenticationEventAsync(clientId, EventConstants.ClientTypes.Scope);
        }

        private async Task RaiseFailureEvent(string clientId, string message)
        {
            await _events.RaiseFailureClientAuthenticationEventAsync(message, clientId, EventConstants.ClientTypes.Scope);
        }
    }
}