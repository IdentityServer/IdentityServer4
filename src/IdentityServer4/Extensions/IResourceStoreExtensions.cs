// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    public static class IResourceStoreExtensions
    {
        public static async Task<Resources> FindResourcesAsync(this IResourceStore store, IEnumerable<string> names)
        {
            var identity = await store.FindIdentityResourcesAsync(names);
            var api = await store.FindApiResourcesByScopeAsync(names);
            var resources = new Resources(identity, api)
            {
                OfflineAccess = names.Contains(Constants.StandardScopes.OfflineAccess)
            };
            return resources;
        }

        public async static Task<Resources> FindEnabledResourcesAsync(this IResourceStore store, IEnumerable<string> names)
        {
            return (await store.FindResourcesAsync(names)).FilterEnabled();
        }

        public static async Task<Resources> GetAllEnabledResourcesAsync(this IResourceStore store)
        {
            var resources = await store.GetAllResources();
            return resources.FilterEnabled();
        }
    }
}