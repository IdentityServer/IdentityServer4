// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Access token options.
    /// </summary>
    public class AccessTokenOptions
    {
        /// <summary>
        /// Specifies whether JWT access tokens issued by IdentityServer will include '~/resources' in the 'aud' claim.
        /// </summary>
        public bool IncludeIssuerResourcesInAudienceClaim { get; set; } = true;
    }
}