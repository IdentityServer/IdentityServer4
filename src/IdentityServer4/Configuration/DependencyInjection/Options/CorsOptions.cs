﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Options for CORS
    /// </summary>
    public class CorsOptions
    {
        /// <summary>
        /// Gets or sets the name of the cors policy.
        /// </summary>
        /// <value>
        /// The name of the cors policy.
        /// </value>
        public string CorsPolicyName { get; set; } = Constants.IdentityServerName;

        /// <summary>
        /// The value to be used in the preflight `Access-Control-Max-Age` response header.
        /// </summary>
        public TimeSpan? PreflightCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the cors paths.
        /// </summary>
        /// <value>
        /// The cors paths.
        /// </value>
        public ICollection<PathString> CorsPaths { get; set; } = Constants.ProtocolRoutePaths.CorsPaths.Select(x => new PathString(x.EnsureLeadingSlash())).ToList();
    }
}