﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Resource retrieval
    /// </summary>
    public interface IResourceStore
    {
        /// <summary>
        /// Gets identity resources by scope name.
        /// </summary>
        Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames);

        /// <summary>
        /// Gets API scopes by scope name.
        /// </summary>
        Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames);
        
        /// <summary>
        /// Gets API resources by scope name.
        /// </summary>
        Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames);

        /// <summary>
        /// Gets API resources by API resource name.
        /// </summary>
        Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames);

        /// <summary>
        /// Gets all resources.
        /// </summary>
        Task<Resources> GetAllResourcesAsync();
    }
}