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

using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Http;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    internal class BearerTokenUsageValidator
    {
        public async Task<BearerTokenUsageValidationResult> ValidateAsync(HttpContext context)
        {
            var result = ValidateAuthorizationHeader(context);
            if (result.TokenFound)
            {
                return result;
            }

            if (context.Request.HasFormContentType)
            {
                result = await ValidatePostBodyAsync(context);
                if (result.TokenFound)
                {
                    return result;
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public BearerTokenUsageValidationResult ValidateAuthorizationHeader(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader.IsPresent())
            {
                var header = authorizationHeader.Trim();
                if (header.StartsWith(Constants.TokenTypes.Bearer))
                {
                    var value = header.Substring(Constants.TokenTypes.Bearer.Length).Trim();
                    if (value != null && value.Length > 0)
                    {
                        return new BearerTokenUsageValidationResult
                        {
                            TokenFound = true,
                            Token = value,
                            UsageType = BearerTokenUsageType.AuthorizationHeader
                        };
                    }
                }
            }

            return new BearerTokenUsageValidationResult();
        }

        public Task<BearerTokenUsageValidationResult> ValidatePostBodyAsync(HttpContext context)
        {
            var token = context.Request.Form["access_token"].FirstOrDefault();
            if (token.IsPresent())
            {
                return Task.FromResult(new BearerTokenUsageValidationResult
                {
                    TokenFound = true,
                    Token = token,
                    UsageType = BearerTokenUsageType.PostBody
                });
            }

            return Task.FromResult(new BearerTokenUsageValidationResult());
        }
    }
}