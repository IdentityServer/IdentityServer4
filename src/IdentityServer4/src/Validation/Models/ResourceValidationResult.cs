// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Result of validation of requested scopes and resource indicators.
    /// </summary>
    public class ResourceValidationResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded => ValidatedResources != null;

        /// <summary>
        /// 
        /// </summary>
        public Resources ValidatedResources { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> ValidScopes { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Collection of scopes that are invalid.
        /// </summary>
        public ICollection<string> InvalidScopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Collection of scopes that are invalid for the client.
        /// </summary>
        public ICollection<string> InvalidScopesForClient { get; set; } = new HashSet<string>();
    }
}