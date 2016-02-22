// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints.Results
{
    internal class TokenResult : IEndpointResult
    {
        public TokenResponse TokenResponse { get; set; }

        public TokenResult(TokenResponse response)
        {
            TokenResponse = response;
        }

        public async Task ExecuteAsync(IdentityServerContext context)
        {
            var dto = new TokenResponseDto
            {
                id_token = TokenResponse.IdentityToken,
                access_token = TokenResponse.AccessToken,
                refresh_token = TokenResponse.RefreshToken,
                expires_in = TokenResponse.AccessTokenLifetime,
                token_type = OidcConstants.TokenTypes.Bearer
            };

            context.HttpContext.Response.SetNoCache();
            await context.HttpContext.Response.WriteJsonAsync(dto);
        }

        internal class TokenResponseDto
        {
            public string id_token { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
        }
    }
}