// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
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

            var request = new ValidatedDeviceAuthorizationRequest
            {
                Raw = parameters ?? throw new ArgumentNullException(nameof(parameters)),
                Options = _options
            };

            var clientResult = ValidateClient(request, clientValidationResult);
            if (clientResult.IsError)
            {
                return clientResult;
            }

            var scopeResult = await ValidateScopeAsync(request);
            if (scopeResult.IsError)
            {
                return scopeResult;
            }

            _logger.LogDebug("{clientId} device authorization request validation success", request.Client.ClientId);
            return Valid(request);
        }

        private DeviceAuthorizationRequestValidationResult Valid(ValidatedDeviceAuthorizationRequest request)
        {
            return new DeviceAuthorizationRequestValidationResult(request);
        }

        private DeviceAuthorizationRequestValidationResult Invalid(ValidatedDeviceAuthorizationRequest request, string error = OidcConstants.AuthorizeErrors.InvalidRequest, string description = null)
        {
            return new DeviceAuthorizationRequestValidationResult(request, error, description);
        }

        private void LogError(string message, ValidatedDeviceAuthorizationRequest request)
        {
            var requestDetails = new DeviceAuthorizationRequestValidationLog(request);
            _logger.LogError(message + "\n{requestDetails}", requestDetails);
        }

        private void LogError(string message, string detail, ValidatedDeviceAuthorizationRequest request)
        {
            var requestDetails = new DeviceAuthorizationRequestValidationLog(request);
            _logger.LogError(message + ": {detail}\n{requestDetails}", detail, requestDetails);
        }

        private DeviceAuthorizationRequestValidationResult ValidateClient(ValidatedDeviceAuthorizationRequest request, ClientSecretValidationResult clientValidationResult)
        {
            //////////////////////////////////////////////////////////
            // set client & secret
            //////////////////////////////////////////////////////////
            if (clientValidationResult == null) throw new ArgumentNullException(nameof(clientValidationResult));
            request.SetClient(clientValidationResult.Client, clientValidationResult.Secret);

            //////////////////////////////////////////////////////////
            // check if client protocol type is oidc
            //////////////////////////////////////////////////////////
            if (request.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.OpenIdConnect)
            {
                LogError("Invalid protocol type for OIDC authorize endpoint", request.Client.ProtocolType, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, "Invalid protocol");
            }

            //////////////////////////////////////////////////////////
            // check if client allows device flow
            //////////////////////////////////////////////////////////
            if (!request.Client.AllowedGrantTypes.Contains(GrantType.DeviceFlow))
            {
                LogError("Client not configured for device flow", GrantType.DeviceFlow, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient);
            }

            return Valid(request);
        }

        private async Task<DeviceAuthorizationRequestValidationResult> ValidateScopeAsync(ValidatedDeviceAuthorizationRequest request)
        {
            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = request.Raw.Get(OidcConstants.AuthorizeRequest.Scope);
            if (scope.IsMissing())
            {
                // TODO: Get all scopes for client, don't error (scope param is optional for device flow)
                LogError("scope is missing", request);
                return Invalid(request, description: "Invalid scope");
            }

            if (scope.Length > _options.InputLengthRestrictions.Scope)
            {
                LogError("scopes too long.", request);
                return Invalid(request, description: "Invalid scope");
            }

            request.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();

            if (request.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OpenId))
            {
                request.IsOpenIdRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported
            //////////////////////////////////////////////////////////
            if (await _scopeValidator.AreScopesValidAsync(request.RequestedScopes) == false)
            {
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope);
            }

            if (_scopeValidator.ContainsOpenIdScopes && !request.IsOpenIdRequest)
            {
                LogError("Identity related scope requests, but no openid scope", request);
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope);
            }

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            if (await _scopeValidator.AreScopesAllowedAsync(request.Client, request.RequestedScopes) == false)
            {
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, "Invalid scope");
            }

            request.ValidatedScopes = _scopeValidator;
            
            return Valid(request);
        }
    }
}