// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


#pragma warning disable 1591

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// Response to request of device flow rfc8628
    /// </summary>
    public class DeviceAuthorizationResponse
    {
        /// <summary>
        /// Code not shown to the user but used to attempt to retrieve tokens via token endpoint with grant type urn:ietf:params:oauth:grant-type:device_code
        /// </summary>
        public string DeviceCode { get; set; }
        
        /// <summary>
        /// The code which the user has to enter to allow this device.
        /// </summary>
        public string UserCode { get; set; }
        
        /// <summary>
        /// VerificationUri to be opened in browser without UserCode
        /// </summary>
        public string VerificationUri { get; set; }

        /// <summary>
        /// VerificationUri to be opened in browser including the UserCode
        /// </summary>
        public string VerificationUriComplete { get; set; }
        
        /// <summary>
        /// Lifetime in seconds until this device flow request expires.
        /// </summary>
        public int DeviceCodeLifetime { get; set; }
        
        /// <summary>
        /// The minimum amount of time in seconds needs to wait between polling requests to the token endpoint.
        /// </summary>
        public int Interval { get; set; }
    }
}