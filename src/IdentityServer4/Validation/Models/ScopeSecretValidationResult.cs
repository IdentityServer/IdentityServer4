// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Validation result for client validation
    /// </summary>
    public class ScopeSecretValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Scope Scope { get; set; }
    }
}