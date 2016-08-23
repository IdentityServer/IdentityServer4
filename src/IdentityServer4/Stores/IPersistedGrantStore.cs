// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Store interface for persisting grants.
    /// </summary>
    public interface IPersistedGrantStore
    {
        Task StoreAsync(PersistedGrant token);
        Task<PersistedGrant> GetAsync(string key);
        Task RemoveAsync(string key);
    }
}
