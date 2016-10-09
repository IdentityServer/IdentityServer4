// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Caching options.
    /// </summary>
    public class CachingOptions
    {
        static TimeSpan _Default = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Gets or sets the client store expiration.
        /// </summary>
        /// <value>
        /// The client store expiration.
        /// </value>
        public TimeSpan ClientStoreExpiration { get; set; } = _Default;

        /// <summary>
        /// Gets or sets the scope store expiration.
        /// </summary>
        /// <value>
        /// The scope store expiration.
        /// </value>
        public TimeSpan ScopeStoreExpiration { get; set; } = _Default;

        /// <summary>
        /// Gets or sets the token validation expiration.
        /// </summary>
        /// <value>
        /// The token validation expiration.
        /// </value>
        public TimeSpan TokenValidationExpiration { get; set; } = TimeSpan.FromMinutes(1);
    }
}