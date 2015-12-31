// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models a claim that should be emitted in a token
    /// </summary>
    public class ScopeClaim
    {
        /// <summary>
        /// Name of the claim
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the claim
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only. Defaults to false.
        /// </summary>
        public bool AlwaysIncludeInIdToken { get; set; }

        /// <summary>
        /// Creates an empty ScopeClaim
        /// </summary>
        public ScopeClaim()
        { }

        /// <summary>
        /// Creates a ScopeClaim with parameters
        /// </summary>
        /// <param name="name">Name of the claim</param>
        /// <param name="alwaysInclude">Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only.</param>
        public ScopeClaim(string name, bool alwaysInclude = false)
        {
            Name = name;
            Description = string.Empty;
            AlwaysIncludeInIdToken = alwaysInclude;
        }
    }
}