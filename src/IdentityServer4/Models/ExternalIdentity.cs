// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the identity of a user authenticating from an external identity provider.
    /// </summary>
    public class ExternalIdentity
    {
        /// <summary>
        /// Identifier of the external identity provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public string Provider { get; set; }


        /// <summary>
        /// User's unique identifier provided by the external identity provider.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        public string ProviderId { get; set; }

        /// <summary>
        /// Claims supplied for the user from the external identity provider.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }

        /// <summary>
        /// Creates an ExternalIdentity and determines the Provider and ProviderId from the list of claims.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">claims</exception>
        public static ExternalIdentity FromClaims(IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException("claims");

            var subClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            claims = claims.Except(new[] { subClaim });
            
            return new ExternalIdentity
            {
                Provider = subClaim.Issuer,
                ProviderId = subClaim.Value,
                Claims = claims
            };
        }
    }
}