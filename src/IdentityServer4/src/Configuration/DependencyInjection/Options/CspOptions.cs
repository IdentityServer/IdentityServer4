// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Options for Content Security Policy
    /// </summary>
    public class CspOptions
    {
        /// <summary>
        /// Gets or sets the minimum CSP level.
        /// </summary>
        public CspLevel Level { get; set; } = CspLevel.Two;

        /// <summary>
        /// Gets or sets own 'default-src' rules.
        /// </summary>
        public string DefaultSrc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets own 'script-src' rules.
        /// </summary>
        public string ScriptSrc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets own 'style-src' rules.
        /// </summary>
        public string StyleSrc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the deprected X-Content-Security-Policy header should be added.
        /// </summary>
        public bool AddDeprecatedHeader { get; set; } = true;
    }
}