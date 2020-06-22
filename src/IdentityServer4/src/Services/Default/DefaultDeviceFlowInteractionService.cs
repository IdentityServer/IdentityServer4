// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    internal class DefaultDeviceFlowInteractionService : IDeviceFlowInteractionService
    {
        private readonly IClientStore _clients;
        private readonly IUserSession _session;
        private readonly IDeviceFlowCodeService _devices;
        private readonly IResourceStore _resourceStore;
        private readonly IScopeParser _scopeParser;
        private readonly ILogger<DefaultDeviceFlowInteractionService> _logger;

        public DefaultDeviceFlowInteractionService(
            IClientStore clients,
            IUserSession session,
            IDeviceFlowCodeService devices,
            IResourceStore resourceStore,
            IScopeParser scopeParser,
            ILogger<DefaultDeviceFlowInteractionService> logger)
        {
            _clients = clients;
            _session = session;
            _devices = devices;
            _resourceStore = resourceStore;
            _scopeParser = scopeParser;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<DeviceFlowAuthorizationRequest> GetAuthorizationContextAsync(string userCode, CancellationToken cancellationToken = default)
        {
            var deviceAuth = await _devices.FindByUserCodeAsync(userCode, cancellationToken);
            if (deviceAuth == null) return null;

            var client = await _clients.FindClientByIdAsync(deviceAuth.ClientId, cancellationToken);
            if (client == null) return null;

            var parsedScopesResult = _scopeParser.ParseScopeValues(deviceAuth.RequestedScopes);
            var validatedResources = await _resourceStore.CreateResourceValidationResult(parsedScopesResult, cancellationToken);

            return new DeviceFlowAuthorizationRequest
            {
                Client = client,
                ValidatedResources = validatedResources
            };
        }

        /// <inheritdoc/>
        public async Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode, ConsentResponse consent, CancellationToken cancellationToken = default)
        {
            if (userCode == null) throw new ArgumentNullException(nameof(userCode));
            if (consent == null) throw new ArgumentNullException(nameof(consent));
            
            var deviceAuth = await _devices.FindByUserCodeAsync(userCode, cancellationToken);
            if (deviceAuth == null) return LogAndReturnError("Invalid user code", "Device authorization failure - user code is invalid");

            var client = await _clients.FindClientByIdAsync(deviceAuth.ClientId, cancellationToken);
            if (client == null) return LogAndReturnError("Invalid client", "Device authorization failure - requesting client is invalid");

            var subject = await _session.GetUserAsync();
            if (subject == null) return LogAndReturnError("No user present in device flow request", "Device authorization failure - no user found");
            
            var sid = await _session.GetSessionIdAsync();

            deviceAuth.IsAuthorized = true;
            deviceAuth.Subject = subject;
            deviceAuth.SessionId = sid;
            deviceAuth.Description = consent.Description;
            deviceAuth.AuthorizedScopes = consent.ScopesValuesConsented;

            // TODO: Device Flow - Record consent template
            if (consent.RememberConsent)
            {
                //var consentRequest = new ConsentRequest(request, subject);
                //await _consentMessageStore.WriteAsync(consentRequest.Id, new Message<ConsentResponse>(consent, _clock.UtcNow.UtcDateTime));
            }

            await _devices.UpdateByUserCodeAsync(userCode, deviceAuth, cancellationToken);

            return new DeviceFlowInteractionResult();
        }

        private DeviceFlowInteractionResult LogAndReturnError(string error, string errorDescription = null)
        {
            _logger.LogError(errorDescription);
            return DeviceFlowInteractionResult.Failure(error);
        }
    }
}