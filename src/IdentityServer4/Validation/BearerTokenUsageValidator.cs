// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Http;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public class BearerTokenUsageValidator
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
                if (header.StartsWith(OidcConstants.TokenTypes.Bearer))
                {
                    var value = header.Substring(OidcConstants.TokenTypes.Bearer.Length).Trim();
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