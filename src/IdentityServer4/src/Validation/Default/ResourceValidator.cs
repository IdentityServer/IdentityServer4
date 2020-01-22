// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default implementation of IResourceValidator.
    /// </summary>
    public class ResourceValidator : IResourceValidator
    {
        private readonly ILogger _logger;
        private readonly IResourceStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceValidator"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="logger">The logger.</param>
        public ResourceValidator(IResourceStore store, ILogger<ResourceValidator> logger)
        {
            _logger = logger;
            _store = store;
        }

        /// <inheritdoc/>
        public async Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new ResourceValidationResult();

            var offlineAccess = request.ScopeValues.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);
            if (offlineAccess)
            {
                if (await IsClientAllowedOfflineAccessAsync(request.Client))
                {
                    result.Scopes.Add(new ScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess));
                }
                else
                {
                    result.InvalidScopesForClient.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
            }

            // filter here so below in our validation loop we're not doing extra checking for offline_access
            var requestedScopes = offlineAccess ?
                request.ScopeValues.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess).ToArray() : 
                request.ScopeValues;

            // todo: brock add ValidateScopeValues?

            var scopeNames = GetScopeNamesFromValues(requestedScopes);
            var names = scopeNames.Select(x => x.Name).Distinct();

            var resources = await _store.FindEnabledResourcesByScopeAsync(names);

            resources.OfflineAccess = offlineAccess;

            foreach (var scope in scopeNames)
            {
                await ValidateScopeAsync(request.Client, resources, scope, result);
            }

            if (result.InvalidScopes.Count > 0 || result.InvalidScopesForClient.Count > 0)
            {
                if (result.InvalidScopes.Count > 0)
                {
                    _logger.LogError("Invalid scopes: {scopes}", result.InvalidScopes);
                }
                if (result.InvalidScopesForClient.Count > 0)
                {
                    _logger.LogError("Invalid scopes for client id: {clientId}, scopes: {scopes}", request.Client.ClientId, result.InvalidScopesForClient);
                }

                result.IdentityResources.Clear();
                result.ApiResources.Clear();
                result.Scopes.Clear();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeValues"></param>
        /// <returns></returns>
        protected IEnumerable<ScopeName> GetScopeNamesFromValues(IEnumerable<string> scopeValues)
        {
            //if (requestedScopes.Contains("payment:"))
            //{
            //    var newScopes = requestedScopes.Where(x => !x.StartsWith("payment:")).ToList();
            //    newScopes.Add("payment");
            //}

            // requestedScopes=["payment:123", "api1", "openid"]
            // client:{allowedScopes="payment"}
            // apiResource {  name="payment",  scopes=[ {name="payment:123", userClaims=['email']} ]}            // todo: keep original scope string, formal name in db, bag for data, scope value to emit

            //result.ApiResources.Where(x=>x.Name == "payment")["txId"] = 

            
            return scopeValues.Select(x => new ScopeName { Name = x, Value = x }).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resources"></param>
        /// <param name="scopeName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task ValidateScopeAsync(Client client, Resources resources, ScopeName scopeName, ResourceValidationResult result)
        {
            var identity = resources.FindIdentityResourcesByScope(scopeName.Name);
            if (identity != null)
            {
                if (!await IsClientAllowedIdentityResourceAsync(client, identity))
                {
                    result.InvalidScopesForClient.Add(scopeName.Value);
                }
                else
                {
                    result.IdentityResources.Add(identity);
                }
            }
            else
            {
                var apiScope = resources.FindScope(scopeName.Name);
                if (apiScope != null)
                {
                    if (!await IsClientAllowedApiScopeAsync(client, apiScope))
                    {
                        result.InvalidScopesForClient.Add(scopeName.Value);
                    }
                    else
                    {
                        result.Scopes.Add(new ScopeValue(scopeName.Name, apiScope));

                        var apis = resources.FindApiResourcesByScope(apiScope.Name);
                        foreach(var api in apis)
                        {
                            resources.ApiResources.Add(api);
                        }
                    }
                }
                else
                {
                    result.InvalidScopes.Add(scopeName.Value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedIdentityResourceAsync(Client client, IdentityResource identity)
        {
            var allowed = client.AllowedScopes.Contains(identity.Name);
            return Task.FromResult(allowed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apiScope"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedApiScopeAsync(Client client, Scope apiScope)
        {
            var allowed = client.AllowedScopes.Contains(apiScope.Name);
            return Task.FromResult(allowed);
        }

        /// <summary>
        /// Validates if the client is allowed offline_access.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedOfflineAccessAsync(Client client)
        {
            return Task.FromResult(client.AllowOfflineAccess);
        }

        public class ScopeName
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}