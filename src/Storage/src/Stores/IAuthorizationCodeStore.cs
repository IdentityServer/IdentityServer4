// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for the authorization code store
    /// </summary>
    public interface IAuthorizationCodeStore
    {
        /// <summary>
        /// Stores the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<AuthorizationCode> GetAuthorizationCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task RemoveAuthorizationCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}