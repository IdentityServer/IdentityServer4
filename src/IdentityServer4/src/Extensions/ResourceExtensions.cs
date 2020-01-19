// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Extensions for Resource
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Returns the collection of scope names that are required.
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetRequiredScopeNames(this Resources resources)
        {
            var identity = resources.IdentityResources.Where(x => x.Required).Select(x => x.Name);
            var apiQuery = from api in resources.ApiResources
                           where api.Scopes != null
                           from scope in api.Scopes
                           where scope.Required
                           select scope.Name;

            var requiredScopes = identity.Union(apiQuery.Distinct());
            return requiredScopes;
        }

        /// <summary>
        /// Returns a new Resources filtered by the scopes indicated.
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public static Resources Filter(this Resources resources, IEnumerable<string> scopes)
        {
            scopes = scopes ?? Enumerable.Empty<string>();

            var offline = scopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);
            if (offline)
            {
                scopes = scopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
            }

            var identityToKeep = resources.IdentityResources.Where(x => x.Required || scopes.Contains(x.Name));
            var apisToKeep = from api in resources.ApiResources
                             where api.Scopes != null
                             let scopesToKeep = (from scope in api.Scopes
                                                 where scope.Required == true || scopes.Contains(scope.Name)
                                                 select scope)
                             where scopesToKeep.Any()
                             select api.CloneWithScopes(scopesToKeep);

            var result = new Resources(identityToKeep, apisToKeep)
            {
                OfflineAccess = offline
            };
            return result;
        }
        
        /// <summary>
                 /// Converts to scope names.
                 /// </summary>
                 /// <param name="resources">The resources.</param>
                 /// <returns></returns>
        public static IEnumerable<string> ToScopeNames(this Resources resources)
        {
            var scopes = resources.IdentityResources.Select(x => x.Name).ToList();
            
            scopes.AddRange(resources.ApiResources.SelectMany(x => x.ToScopeNames()).Distinct());
            
            if (resources.OfflineAccess)
            {
                scopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
            }
            
            return scopes;
        }

        /// <summary>
        /// Converts to scope names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> ToScopeNames(this ApiResource apiResource)
        {
            if (apiResource == null)
            {
                throw new ArgumentNullException(nameof(apiResource));
            }

            if (apiResource.Scopes == null)
            {
                return Enumerable.Empty<string>();
            }

            var scopes = from scope in apiResource.Scopes
                         select scope.Name;

            return scopes.ToArray();
        }

        /// <summary>
        /// Finds the API resources that contain the scope.
        /// </summary>
        /// <param name="resources">The resources.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static IEnumerable<ApiResource> FindApiResourcesByScope(this Resources resources, string name)
        {
            var q = from api in resources.ApiResources
                    where api.Scopes != null
                    from scope in api.Scopes
                    where scope.Name == name
                    select api;
            return q.ToArray();
        }

        /// <summary>
        /// Finds the API scope.
        /// </summary>
        /// <param name="resources">The resources.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Scope FindApiScope(this Resources resources, string name)
        {
            var q = from api in resources.ApiResources
                    where api.Scopes != null
                    from scope in api.Scopes
                    where scope.Name == name
                    select scope;
            return q.FirstOrDefault();
        }

        /// <summary>
        /// Finds the API scope.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Scope FindApiScope(this ApiResource api, string name)
        {
            if (api == null || api.Scopes == null) return null;

            var q = from s in api.Scopes
                    where s.Name == name
                    select s;
            return q.FirstOrDefault();
        }

        internal static Resources FilterEnabled(this Resources resources)
        {
            if (resources == null) return new Resources();

            var identity = resources.IdentityResources.Where(x => x.Enabled);

            var api = from a in resources.ApiResources
                      where a.Enabled
                      select a;

            return new Resources(identity, api)
            {
                OfflineAccess = resources.OfflineAccess
            };
        }

        internal static ICollection<string> FindMatchingSigningAlgorithms(this IEnumerable<ApiResource> apiResources)
        {
            var apis = apiResources.ToList();

            if (apis.IsNullOrEmpty())
            {
                return new List<string>();
            }

            // only one API resource request, forward the allowed signing algorithms (if any)
            if (apis.Count == 1)
            {
                return apis.First().AllowedAccessTokenSigningAlgorithms;
            }
            
            var allAlgorithms = apis.Where(r => r.AllowedAccessTokenSigningAlgorithms.Any()).Select(r => r.AllowedAccessTokenSigningAlgorithms).ToList();

            // resources need to agree on allowed signing algorithms
            if (allAlgorithms.Any())
            {
                var allowedAlgorithms = IntersectLists(allAlgorithms);

                if (allowedAlgorithms.Any())
                {
                    return allowedAlgorithms.ToHashSet();
                }
                else
                {
                    throw new InvalidOperationException("Signing algorithms requirements for requested resources are not compatible.");
                }
            }

            return new List<string>();
        }

        private static IEnumerable<T> IntersectLists<T>(IEnumerable<IEnumerable<T>> lists)
        {
            return lists.Aggregate((l1, l2) => l1.Intersect(l2));
        }

        internal static ResourceValidationResult ToResourceValidationResult(this Resources resources)
        {
            var scopes = new List<ValidatedScope>();

            if (resources.OfflineAccess)
            {
                scopes.Add(new ValidatedScope(IdentityServerConstants.StandardScopes.OfflineAccess));
            }

            foreach (var scope in resources.ToScopeNames())
            {
                var apiScope = resources.FindApiScope(scope);
                if (apiScope != null)
                {
                    var apis = resources.FindApiResourcesByScope(apiScope.Name);
                    scopes.Add(new ValidatedScope(scope, apis, apiScope));
                }
                else
                {
                    var id = resources.IdentityResources.FirstOrDefault(x => x.Name == scope);
                    if (id != null)
                    {
                        scopes.Add(new ValidatedScope(scope, id));
                    }
                }
            }

            var validatedResource = new ResourceValidationResult
            {
                ValidScopes = scopes
            };

            return validatedResource;
        }
    }
}