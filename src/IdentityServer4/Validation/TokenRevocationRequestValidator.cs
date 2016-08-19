// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    public class TokenRevocationRequestValidator : ITokenRevocationRequestValidator
    {
        public Task<TokenRevocationRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, Client client)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (client == null) throw new ArgumentNullException(nameof(client));

            ////////////////////////////
            // make sure token is present
            ///////////////////////////
            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                return Task.FromResult(new TokenRevocationRequestValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.TokenErrors.InvalidRequest
                });
            }

            var result = new TokenRevocationRequestValidationResult
            {
                IsError = false,
                Token = token
            };

            ////////////////////////////
            // check token type hint
            ///////////////////////////
            var hint = parameters.Get("token_type_hint");
            if (hint.IsPresent())
            {
                if (Constants.SupportedTokenTypeHints.Contains(hint))
                {
                    result.TokenTypeHint = hint;
                }
                else
                {
                    result.IsError = true;
                    result.Error = Constants.RevocationErrors.UnsupportedTokenType;
                }
            }

            return Task.FromResult(result);
        }
    }
}