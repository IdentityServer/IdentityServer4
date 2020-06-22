// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Retrieval of client configuration
    /// </summary>
    public interface IClientStore
    {
        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The client</returns>
        Task<Client> FindClientByIdAsync(string clientId, CancellationToken cancellationToken = default);
    }
}