// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Default custom request validator
    /// </summary>
    public class DefaultCustomRequestValidator : ICustomRequestValidator
    {
        /// <summary>
        /// Custom validation logic for the authorize request.
        /// </summary>
        /// <param name="request">The validated request.</param>
        /// <returns>
        /// The validation result
        /// </returns>
        // todo
        public Task<AuthorizeRequestValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(new AuthorizeRequestValidationResult
            {
                IsError = false
            });
        }

        /// <summary>
        /// Custom validation logic for the token request.
        /// </summary>
        /// <param name="request">The validated request.</param>
        /// <returns>
        /// The validation result
        /// </returns>
        public Task<TokenRequestValidationResult> ValidateTokenRequestAsync(ValidatedTokenRequest request)
        {
            return Task.FromResult(new TokenRequestValidationResult(request));
        }
    }
}