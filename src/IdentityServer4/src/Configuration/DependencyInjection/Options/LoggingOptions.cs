// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityModel;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Options for configuring logging behavior
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public ICollection<string> TokenRequestSensitiveValuesFilter { get; set; } = 
            new HashSet<string>
            {
                OidcConstants.TokenRequest.ClientSecret,
                OidcConstants.TokenRequest.Password,
                OidcConstants.TokenRequest.ClientAssertion,
                OidcConstants.TokenRequest.RefreshToken,
                OidcConstants.TokenRequest.DeviceCode
            };

        /// <summary>
        /// 
        /// </summary>
        public ICollection<string> AuthorizeRequestSensitiveValuesFilter { get; set; } = 
            new HashSet<string>
            {
                OidcConstants.AuthorizeRequest.IdTokenHint
            };
    }
}