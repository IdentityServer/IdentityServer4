// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// The device authorizaiton response generator
    /// </summary>
    /// <seealso cref="IdentityServer4.ResponseHandling.IDeviceAuthorizationResponseGenerator" />
    public class DeviceAuthorizationResponseGenerator : IDeviceAuthorizationResponseGenerator
    {
        /// <summary>
        /// The options
        /// </summary>
        protected readonly IdentityServerOptions Options;

        /// <summary>
        /// The user code service
        /// </summary>
        protected readonly IUserCodeService UserCodeService;

        /// <summary>
        /// The device flow store
        /// </summary>
        protected readonly IDeviceFlowStore DeviceFlowStore;

        /// <summary>
        /// The clock
        /// </summary>
        protected readonly ISystemClock Clock;

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAuthorizationResponseGenerator"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="userCodeService">The user code service.</param>
        /// <param name="deviceFlowStore">The device flow store.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        public DeviceAuthorizationResponseGenerator(IdentityServerOptions options, IUserCodeService userCodeService, IDeviceFlowStore deviceFlowStore, ISystemClock clock, ILogger<DeviceAuthorizationResponseGenerator> logger)
        {
            Options = options;
            UserCodeService = userCodeService;
            DeviceFlowStore = deviceFlowStore;
            Clock = clock;
            Logger = logger;
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">validationResult or Client</exception>
        /// <exception cref="ArgumentException">Value cannot be null or whitespace. - baseUrl</exception>
        public virtual async Task<DeviceAuthorizationResponse> ProcessAsync(DeviceAuthorizationRequestValidationResult validationResult, string baseUrl)
        {
            if (validationResult == null) throw new ArgumentNullException(nameof(validationResult));
            if (validationResult.ValidatedRequest.Client == null) throw new ArgumentNullException(nameof(validationResult.ValidatedRequest.Client));
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(baseUrl));

            Logger.LogTrace("Creating response for device authorization request");

            var response = new DeviceAuthorizationResponse();
            
            // generate user_code
            var userCodeGenerator = await UserCodeService.GetGenerator(validationResult.ValidatedRequest.Client.UserCodeType ?? Options.DeviceFlow.DefaultUserCodeType);
            response.UserCode = await userCodeGenerator.GenerateAsync();

            // generate verification URIs
            response.VerificationUri = baseUrl.RemoveTrailingSlash() + Options.UserInteraction.DeviceVerificationUrl;
            if (!string.IsNullOrWhiteSpace(Options.UserInteraction.DeviceVerificationUserCodeParameter))
                response.VerificationUriComplete = $"{response.VerificationUri}?{Options.UserInteraction.DeviceVerificationUserCodeParameter}={response.UserCode}";

            // expiration
            response.DeviceCodeLifetime = validationResult.ValidatedRequest.Client.DeviceCodeLifetime;

            // interval
            response.Interval = Options.DeviceFlow.Interval;

            // store device request (device code & user code)
            response.DeviceCode = await DeviceFlowStore.StoreDeviceAuthorizationAsync(response.UserCode, new DeviceCode
            {
                ClientId = validationResult.ValidatedRequest.Client.ClientId,
                IsOpenId = validationResult.ValidatedRequest.IsOpenIdRequest,
                Lifetime = response.DeviceCodeLifetime,
                CreationTime = Clock.UtcNow.UtcDateTime,
                RequestedScopes = validationResult.ValidatedRequest.RequestedScopes
            });

            return response;
        }
    }
}