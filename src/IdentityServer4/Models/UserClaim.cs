// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using System;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models a claim that should be emitted in a token
    /// </summary>
    public class UserClaim
    {
        /// <summary>
        /// Creates an empty ScopeClaim
        /// </summary>
        public UserClaim()
        { }

        /// <summary>
        /// Creates a ScopeClaim with parameters
        /// </summary>
        /// <param name="type">Type of the claim</param>
        /// <param name="alwaysInclude">Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only.</param>
        public UserClaim(string type, bool alwaysInclude = false)
        {
            if (type.IsMissing()) throw new ArgumentNullException(nameof(type));

            Type = type;
            Description = string.Empty;
            AlwaysIncludeInIdToken = alwaysInclude;
        }
        
        /// <summary>
        /// Type of the claim
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Description of the claim
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only. Defaults to false.
        /// </summary>
        public bool AlwaysIncludeInIdToken { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Type?.GetHashCode() ?? 0;

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as UserClaim;
            if (obj == null) return false;
            if (Object.ReferenceEquals(other, this)) return true;

            return String.Equals(other.Type, Type, StringComparison.Ordinal);
        }
    }
}