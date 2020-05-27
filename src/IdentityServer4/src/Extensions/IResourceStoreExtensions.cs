// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using IdentityServer4.Validation;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Extensions for IResourceStore
    /// </summary>
    public static class IResourceStoreExtensions
    {
        /// <summary>
        /// Finds the resources by scope.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="scopeNames">The scope names.</param>
        /// <returns></returns>
        public static async Task<Resources> FindResourcesByScopeAsync(this IResourceStore store, IEnumerable<string> scopeNames)
        {
            var identity = await store.FindIdentityResourcesByScopeNameAsync(scopeNames);
            var apiResources = await store.FindApiResourcesByScopeNameAsync(scopeNames);
            var scopes = await store.FindApiScopesByNameAsync(scopeNames);

            Validate(identity, apiResources, scopes);

            var resources = new Resources(identity, apiResources, scopes)
            {
                OfflineAccess = scopeNames.Contains(IdentityServerConstants.StandardScopes.OfflineAccess)
            };

            return resources;
        }

        private static void Validate(IEnumerable<IdentityResource> identity, IEnumerable<ApiResource> apiResources, IEnumerable<ApiScope> apiScopes)
        {
            // attempt to detect invalid configuration. this is about the only place
            // we can do this, since it's hard to get the values in the store.
            var identityScopeNames = identity.Select(x => x.Name).ToArray();
            var dups = GetDuplicates(identityScopeNames);
            if (dups.Any())
            {
                var names = dups.Aggregate((x, y) => x + ", " + y);
                throw new Exception(
                    $"Duplicate identity scopes found. This is an invalid configuration. Use different names for identity scopes. Scopes found: {names}");
            }

            var apiNames = apiResources.Select(x => x.Name);
            dups = GetDuplicates(apiNames);
            if (dups.Any())
            {
                var names = dups.Aggregate((x, y) => x + ", " + y);
                throw new Exception(
                    $"Duplicate api resources found. This is an invalid configuration. Use different names for API resources. Names found: {names}");
            }
            
            var scopesNames = apiScopes.Select(x => x.Name);
            dups = GetDuplicates(scopesNames);
            if (dups.Any())
            {
                var names = dups.Aggregate((x, y) => x + ", " + y);
                throw new Exception(
                    $"Duplicate scopes found. This is an invalid configuration. Use different names for scopes. Names found: {names}");
            }

            var overlap = identityScopeNames.Intersect(scopesNames).ToArray();
            if (overlap.Any())
            {
                var names = overlap.Aggregate((x, y) => x + ", " + y);
                throw new Exception(
                    $"Found identity scopes and API scopes that use the same names. This is an invalid configuration. Use different names for identity scopes and API scopes. Scopes found: {names}");
            }
        }

        private static IEnumerable<string> GetDuplicates(IEnumerable<string> names)
        {
            var duplicates = names
                            .GroupBy(x => x)
                            .Where(g => g.Count() > 1)
                            .Select(y => y.Key)
                            .ToArray();
            return duplicates.ToArray();
        }

        /// <summary>
        /// Finds the enabled resources by scope.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="scopeNames">The scope names.</param>
        /// <returns></returns>
        public static async Task<Resources> FindEnabledResourcesByScopeAsync(this IResourceStore store, IEnumerable<string> scopeNames)
        {
            return (await store.FindResourcesByScopeAsync(scopeNames)).FilterEnabled();
        }

        /// <summary>
        /// Creates a resource validation result.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="parsedScopeValues">The parsed scopes.</param>
        /// <returns></returns>
        public static async Task<ResourceValidationResult> CreateResourceValidationResult(this IResourceStore store, IEnumerable<ParsedScopeValue> parsedScopeValues)
        {
            var scopes = parsedScopeValues.Select(x => x.Name).ToArray();
            var resources = await store.FindEnabledResourcesByScopeAsync(scopes);
            return new ResourceValidationResult(resources, parsedScopeValues);
        }

        /// <summary>
        /// Gets the names of the scopes
        /// </summary>
        /// <param name="resourceValidator"></param>
        /// <param name="scopeValues">The scope names.</param>
        /// <returns></returns>
        public static async Task<(IEnumerable<ParsedScopeValue> parsedScopes, IEnumerable<string> scopeNames)> GetParsedScopes(this IResourceValidator resourceValidator, IEnumerable<string> scopeValues)
        {
            var parsedScopes = await resourceValidator.ParseRequestedScopesAsync(scopeValues);
            var scopeNames = parsedScopes.Select(x => x.Name).ToArray();
            return (parsedScopes, scopeNames);
        }

        /// <summary>
        /// Gets all enabled resources.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <returns></returns>
        public static async Task<Resources> GetAllEnabledResourcesAsync(this IResourceStore store)
        {
            var resources = await store.GetAllResourcesAsync();
            Validate(resources.IdentityResources, resources.ApiResources, resources.ApiScopes);

            return resources.FilterEnabled();
        }

        /// <summary>
        /// Finds the enabled identity resources by scope.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="scopeNames">The scope names.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<IdentityResource>> FindEnabledIdentityResourcesByScopeAsync(this IResourceStore store, IEnumerable<string> scopeNames)
        {
            return (await store.FindIdentityResourcesByScopeNameAsync(scopeNames)).Where(x => x.Enabled).ToArray();
        }
    }
}
