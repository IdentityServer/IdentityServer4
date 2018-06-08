// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Logging;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Validation
{
    internal class DeviceAuthorizationRequestValidator : IDeviceAuthorizationRequestValidator
    {
        private readonly IdentityServerOptions _options;
        private readonly ScopeValidator _scopeValidator;
        private readonly ILogger<DeviceAuthorizationRequestValidator> _logger;

        private ValidatedDeviceAuthorizationRequest validatedRequest;

        public DeviceAuthorizationRequestValidator(
            IdentityServerOptions options,
            ScopeValidator scopeValidator,
            ILogger<DeviceAuthorizationRequestValidator> logger)
        {
            _options = options;
            _scopeValidator = scopeValidator;
            _logger = logger;
        }

        public async Task<DeviceAuthorizationRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult)
        {
            _logger.LogDebug("Start device authorization request validation");

            validatedRequest = new ValidatedDeviceAuthorizationRequest
            {
                Raw = parameters ?? throw new ArgumentNullException(nameof(parameters)),
                Options = _options
            };

            if (clientValidationResult == null) throw new ArgumentNullException(nameof(clientValidationResult));
            validatedRequest.SetClient(clientValidationResult.Client, clientValidationResult.Secret);

            // check if client Authorized for device flow
            if (!validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.DeviceFlow))
            {
                LogError("Client not authorized for device flow");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }

            if (!await ValidateRequestedScopesAsync(parameters))
            {
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            _logger.LogDebug("{clientId} device authorization request validation success", validatedRequest.Client.ClientId);
            return Valid();
        }

        private DeviceAuthorizationRequestValidationResult Valid()
        {
            return new DeviceAuthorizationRequestValidationResult(validatedRequest);
        }

        private DeviceAuthorizationRequestValidationResult Invalid(string error, string errorDescription = null)
        {
            return new DeviceAuthorizationRequestValidationResult(validatedRequest, error, errorDescription);
        }

        private void LogError(string message = null, params object[] values)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    _logger.LogError(message, values);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error logging {exception}", ex.Message);
                }
            }

            var details = new DeviceAuthorizationRequestValidationLog(validatedRequest);
            _logger.LogError("{details}", details);
        }

        // HACK: Copied from TokenRequestValidator
        private async Task<bool> ValidateRequestedScopesAsync(NameValueCollection parameters)
        {
            var scopes = parameters.Get(OidcConstants.TokenRequest.Scope);

            if (scopes.IsMissing())
            {
                _logger.LogTrace("Client provided no scopes - checking allowed scopes list");

                if (!validatedRequest.Client.AllowedScopes.IsNullOrEmpty())
                {
                    var clientAllowedScopes = new List<string>(validatedRequest.Client.AllowedScopes);
                    if (validatedRequest.Client.AllowOfflineAccess)
                    {
                        clientAllowedScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                    }
                    scopes = clientAllowedScopes.ToSpaceSeparatedString();
                    _logger.LogTrace("Defaulting to: {scopes}", scopes);
                }
                else
                {
                    LogError("No allowed scopes configured for {clientId}", validatedRequest.Client.ClientId);
                    return false;
                }
            }

            if (scopes.Length > _options.InputLengthRestrictions.Scope)
            {
                LogError("Scope parameter exceeds max allowed length");
                return false;
            }

            var requestedScopes = scopes.ParseScopesString();

            if (requestedScopes == null)
            {
                LogError("No scopes found in request");
                return false;
            }

            if (!await _scopeValidator.AreScopesAllowedAsync(validatedRequest.Client, requestedScopes))
            {
                LogError();
                return false;
            }

            if (!(await _scopeValidator.AreScopesValidAsync(requestedScopes)))
            {
                LogError();
                return false;
            }

            validatedRequest.Scopes = requestedScopes;
            validatedRequest.ValidatedScopes = _scopeValidator;
            return true;
        }
    }
}