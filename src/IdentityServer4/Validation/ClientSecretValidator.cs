/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Events;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Http;

namespace IdentityServer4.Core.Validation
{
    public class ClientSecretValidator
    {
        private readonly ILogger _logger;
        private readonly IClientStore _clients;
        private readonly IEventService _events;
        private readonly IEnumerable<ISecretParser> _parsers;
        private readonly IEnumerable<ISecretValidator> _validators;

        public ClientSecretValidator(IClientStore clients, IEnumerable<ISecretParser> parsers, IEnumerable<ISecretValidator> validators, IHttpContextAccessor contextAccessor, IEventService events, ILoggerFactory loggerFactory)
        {
            _clients = clients;
            _parsers = parsers;
            _validators = validators;
            _events = events;
            _logger = loggerFactory.CreateLogger<ClientSecretValidator>();
        }

        public async Task<ClientSecretValidationResult> ValidateAsync(HttpContext context)
        {
            _logger.LogVerbose("Start client validation");

            var fail = new ClientSecretValidationResult
            {
                IsError = true
            };

            // see if a registered parser finds a secret on the request
            ParsedSecret parsedSecret = null;
            foreach (var parser in _parsers)
            {
                parsedSecret = await parser.ParseAsync(context);
                if (parsedSecret != null)
                {
                    _logger.LogVerbose("Parser found client secret: {parser}", parser.GetType().Name);
                    _logger.LogInformation("Client secret id found: {id}", parsedSecret.Id);

                    break;
                }
            }

            if (parsedSecret == null)
            {
                await RaiseFailureEvent("unknown", "No client id or secret found");

                _logger.LogInformation("No client secret found");
                return fail;
            }

            // load client
            var client = await _clients.FindClientByIdAsync(parsedSecret.Id);
            if (client == null)
            {
                await RaiseFailureEvent(parsedSecret.Id, "Unknown client");

                _logger.LogWarning("No client with that id found. aborting");
                return fail;
            }

            // see if a registered validator can validate the secret
            foreach (var validator in _validators)
            {
                var secretValidationResult = await validator.ValidateAsync(client.ClientSecrets, parsedSecret);

                if (secretValidationResult.Success)
                {
                    _logger.LogVerbose("Secret validator success: {validator}", validator.GetType().Name);
                    _logger.LogInformation("Client validation success");

                    var success = new ClientSecretValidationResult
                    {
                        IsError = false,
                        Client = client
                    };

                    await RaiseSuccessEvent(client.ClientId);
                    return success;
                }
            }

            await RaiseFailureEvent(client.ClientId, "Invalid client secret");
            _logger.LogWarning("Client validation failed.");
            
            return fail;
        }

        private async Task RaiseSuccessEvent(string clientId)
        {
            await _events.RaiseSuccessfulClientAuthenticationEventAsync(clientId, EventConstants.ClientTypes.Client);
        }

        private async Task RaiseFailureEvent(string clientId, string message)
        {
            await _events.RaiseFailureClientAuthenticationEventAsync(message, clientId, EventConstants.ClientTypes.Client);
        }
    }
}