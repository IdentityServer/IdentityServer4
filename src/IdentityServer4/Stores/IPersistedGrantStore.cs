// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Store interface for persisting grants.
    /// </summary>
    public interface IPersistedGrantStore
    {
        Task StoreAsync(PersistedGrant grant);

        Task<PersistedGrant> GetAsync(string key);
        Task<IEnumerable<PersistedGrant>> GetAsync(string subjectId, string type);

        Task RemoveAsync(string key);
        Task RemoveAsync(string subjectId, string clientId);
        Task RemoveAsync(string subjectId, string clientId, string type);
    }
}
