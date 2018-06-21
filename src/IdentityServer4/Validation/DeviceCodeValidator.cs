// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Validation
{
    internal class DeviceCodeValidator : IDeviceCodeValidator
    {
        private readonly IDeviceCodeStore _devices;
        private readonly IProfileService _profile;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<DeviceCodeValidator> _logger;

        public DeviceCodeValidator(
            IDeviceCodeStore devices,
            IProfileService profile,
            ISystemClock systemClock,
            ILogger<DeviceCodeValidator> logger)
        {
            _devices = devices;
            _profile = profile;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task ValidateAsync(DeviceCodeValidationContext context)
        {
            var deviceCode = await _devices.GetDeviceCodeAsync(context.DeviceCode);

            if (deviceCode == null)
            {
                _logger.LogError("Invalid device code");
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);
                return;
            }

            // TODO: slow_down
            
            // validate client binding
            if (deviceCode.ClientId != context.Request.Client.ClientId)
            {
                _logger.LogError("Client {0} is trying to use a device code from client {1}", context.Request.Client.ClientId, deviceCode.ClientId);
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);
                return;
            }

            // validate lifetime
            if (deviceCode.CreationTime.AddSeconds(deviceCode.Lifetime) < _systemClock.UtcNow)
            {
                _logger.LogError("Expired device code");
                context.Result = new TokenRequestValidationResult(context.Request, "expired_token");
                return;
            }

            // denied
            if (!deviceCode.AuthorizedScopes.Any())
            {
                _logger.LogError("No scopes authorized for device authorization. Access denied");
                context.Result = new TokenRequestValidationResult(context.Request, "access_denied");
                return;
            }

            // make sure code is authorized
            if (!deviceCode.IsAuthorized || deviceCode.Subject == null)
            {
                context.Result = new TokenRequestValidationResult(context.Request, "authorization_pending");
                return;
            }

            // make sure user is enabled
            var isActiveCtx = new IsActiveContext(deviceCode.Subject, context.Request.Client, IdentityServerConstants.ProfileIsActiveCallers.DeviceCodeValidation);
            await _profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                _logger.LogError("User has been disabled: {subjectId}", deviceCode.Subject.GetSubjectId());
                context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);
                return;
            }

            context.Request.DeviceCode = deviceCode;
            context.Result = new TokenRequestValidationResult(context.Request);
            await _devices.RemoveDeviceCodeAsync(context.DeviceCode);

        }
    }
}