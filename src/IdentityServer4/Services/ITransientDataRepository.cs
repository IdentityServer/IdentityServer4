// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Abstraction for storing transient data (e.g. authorization codes, refresh and reference tokens)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITransientDataRepository<T>
        where T : ITokenMetadata
    {
        /// <summary>
        /// Stores the data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Task StoreAsync(string key, T value);

        /// <summary>
        /// Retrieves the data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        Task<T> GetAsync(string key);

        /// <summary>
        /// Removes the data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Retrieves all data for a subject identifier.
        /// </summary>
        /// <param name="subject">The subject identifier.</param>
        /// <returns>A list of token metadata</returns>
        Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject);

        /// <summary>
        /// Revokes all data for a client and subject id combination.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        Task RevokeAsync(string subject, string client);
    }
}