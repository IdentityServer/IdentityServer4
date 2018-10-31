// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Validation;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// Interface for the device authorization response generator
    /// </summary>
    public interface IDeviceAuthorizationResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        Task<DeviceAuthorizationResponse> ProcessAsync(DeviceAuthorizationRequestValidationResult validationResult, string baseUrl);
    }
}