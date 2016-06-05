// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Scope retrieval
    /// </summary>
    public interface IScopeStore
    {
        /// <summary>
        /// Gets all scopes.
        /// </summary>
        /// <returns>List of scopes</returns>
        Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames);

        /// <summary>
        /// Gets all defined scopes.
        /// </summary>
        /// <param name="publicOnly">if set to <c>true</c> only public scopes are returned.</param>
        /// <returns></returns>
        Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true);
    }
}