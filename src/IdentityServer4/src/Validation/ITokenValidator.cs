// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Interface for the token validator
    /// </summary>
    public interface ITokenValidator
    {
        /// <summary>
        /// Validates an access token.
        /// </summary>
        /// <param name="token">The access token.</param>
        /// <param name="expectedScope">The expected scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates an identity token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="validateLifetime">if set to <c>true</c> the lifetime gets validated. Otherwise not.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true, CancellationToken cancellationToken = default);
    }
}