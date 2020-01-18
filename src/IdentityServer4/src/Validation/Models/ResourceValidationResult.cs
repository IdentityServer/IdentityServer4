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
        /// Indicates if the result was successful.
        /// </summary>
        public bool Succeeded => Resources != null;

        /// <summary>
        /// Resources that model the validated scopes.
        /// </summary>
        public Resources Resources { get; set; }

        /// <summary>
        /// The valid scope values.
        /// </summary>
        public ICollection<string> Scopes { get; set; } = new HashSet<string>();
        
        /// <summary>
        /// The scopes which are required.
        /// </summary>
        public ICollection<string> RequiredScopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// The requested scopes that are invalid.
        /// </summary>
        public ICollection<string> InvalidScopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// The requested scopes that are not allowed for the client.
        /// </summary>
        public ICollection<string> InvalidScopesForClient { get; set; } = new HashSet<string>();
    }
}
