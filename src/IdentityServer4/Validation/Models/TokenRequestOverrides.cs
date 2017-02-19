// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Allows setting per-request configuration overrides for token requests 
    /// (e.g. from a custom token request validator or an extension/password grant validator
    /// </summary>
    public class TokenRequestOverrides
    {
        public AccessTokenType? TokenType { get; set; } = null;
        public int? TokenLifetime { get; set; } = null;

        public ICollection<Claim> ClientClaims { get; set; } = new HashSet<Claim>(new ClaimComparer());
        public ICollection<string> Scopes { get; set; } = new HashSet<string>();
    }
}
