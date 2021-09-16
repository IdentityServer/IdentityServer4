// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Options for cross-origin resource sharing (CORS).
    /// </summary>
    public class CorsOptions
    {
        /// <summary>
        /// Gets or sets the name of the CORS policy.
        /// </summary>
        /// <value>
        /// The name of the CORS policy.
        /// </value>
        public string CorsPolicyName { get; set; } = Constants.IdentityServerName;

        /// <summary>
        /// The value to be used in the preflight `Access-Control-Max-Age` response header.
        /// </summary>
        public TimeSpan? PreflightCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the CORS paths.
        /// </summary>
        /// <value>
        /// The CORS paths.
        /// </value>
        public ICollection<PathString> CorsPaths { get; set; } = Constants.ProtocolRoutePaths.CorsPaths.Select(x => new PathString(x.EnsureLeadingSlash())).ToList();
    }
}
