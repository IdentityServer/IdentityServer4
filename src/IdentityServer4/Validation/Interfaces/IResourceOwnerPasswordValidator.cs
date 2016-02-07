// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Handles validation of resource owner password credentials
    /// </summary>
    public interface IResourceOwnerPasswordValidator
    {
        /// <summary>
        /// Validates the resource owner password credential
        /// </summary>
        /// <param name="request">The validated token request.</param>
        /// <returns>The validation result</returns>
        Task<CustomGrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request);
    }
}