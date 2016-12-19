// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using System.Security.Claims;
using IdentityServer4.Services;

namespace IdentityServer4
{
    public class IdentityServerTools
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenCreationService _tokenCreation;

        public IdentityServerTools(IHttpContextAccessor contextAccessor, ITokenCreationService tokenCreation)
        {
            _tokenCreation = tokenCreation;
            _contextAccessor = contextAccessor;
        }

        public virtual async Task<string> IssueClientTokenAsync(string clientId, int lifetime, IEnumerable<string> scopes = null, IEnumerable<string> audiences = null)
        {
            var issuer = _contextAccessor.HttpContext.GetIdentityServerIssuerUri();

            var token = new Token
            {
                Issuer = issuer,
                Lifetime = lifetime,

                Claims = new List<Claim>
                {
                    new Claim("client_id", clientId),
                }
            };

            token.Audiences.Add(string.Format(Constants.AccessTokenAudience, issuer.EnsureTrailingSlash()));
            if (!audiences.IsNullOrEmpty())
            {
                foreach (var audience in audiences)
                {
                    token.Audiences.Add(audience);
                }
            }

            if (!scopes.IsNullOrEmpty())
            {
                foreach (var scope in scopes)
                {
                    token.Claims.Add(new Claim("scope", scope));
                }
            }

            return await _tokenCreation.CreateTokenAsync(token);
        }
    }
}