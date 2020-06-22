﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for reference token storage
    /// </summary>
    public interface IReferenceTokenStore
    {
        /// <summary>
        /// Stores the reference token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<string> StoreReferenceTokenAsync(Token token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the reference token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<Token> GetReferenceTokenAsync(string handle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the reference token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task RemoveReferenceTokenAsync(string handle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the reference tokens.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task RemoveReferenceTokensAsync(string subjectId, string clientId, CancellationToken cancellationToken = default);
    }
}