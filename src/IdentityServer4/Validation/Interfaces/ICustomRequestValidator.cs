// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Allows inserting custom validation logic into authorize and token requests
    /// </summary>
    public interface ICustomRequestValidator
    {
        /// <summary>
        /// Custom validation logic for the authorize request.
        /// </summary>
        /// <param name="request">The validated request.</param>
        /// <returns>The validation result</returns>
        // postpone
        Task<AuthorizeRequestValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request);

        /// <summary>
        /// Custom validation logic for the token request.
        /// </summary>
        /// <param name="request">The validated request.</param>
        /// <returns>The validation result</returns>
        Task<TokenRequestValidationResult> ValidateTokenRequestAsync(ValidatedTokenRequest request);
    }
}