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
    /// <summary>
    /// Class for userful helpers for interacting with IdentityServer
    /// </summary>
    public class IdentityServerTools
    {
        internal readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenCreationService _tokenCreation;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerTools"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        /// <param name="tokenCreation">The token creation service.</param>
        public IdentityServerTools(IHttpContextAccessor contextAccessor, ITokenCreationService tokenCreation)
        {
            _tokenCreation = tokenCreation;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Issues a JWT.
        /// </summary>
        /// <param name="lifetime">The lifetime.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">claims</exception>
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