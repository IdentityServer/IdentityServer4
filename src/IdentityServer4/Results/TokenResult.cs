/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
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
                token_type = Constants.TokenTypes.Bearer
            };

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