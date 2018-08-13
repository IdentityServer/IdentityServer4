﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Allows inserting custom token validation logic
    /// </summary>
    public interface ICustomTokenValidator
    {
        /// <summary>
        /// Custom validation logic for access tokens.
        /// </summary>
        /// <param name="result">The validation result so far.</param>
        /// <returns>The validation result</returns>
        Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result);

        /// <summary>
        /// Custom validation logic for identity tokens.
        /// </summary>
        /// <param name="result">The validation result so far.</param>
        /// <returns>The validation result</returns>
        Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result);
    }
}