// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using System.Security.Claims;
using IdentityServer4.Services;
using IdentityModel;
using System;

namespace IdentityServer4
{
    public class IdentityServerTools
    {
        internal readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenCreationService _tokenCreation;

        public IdentityServerTools(IHttpContextAccessor contextAccessor, ITokenCreationService tokenCreation)
        {
            _tokenCreation = tokenCreation;
            _contextAccessor = contextAccessor;
        }

        public virtual async Task<string> IssueJwtAsync(int lifetime, IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            var issuer = _contextAccessor.HttpContext.GetIdentityServerIssuerUri();

            var token = new Token
            {
                Issuer = issuer,
                Lifetime = lifetime,

                Claims = new HashSet<Claim>(claims, new ClaimComparer())
            };

            return await _tokenCreation.CreateTokenAsync(token);
        }
    }
}