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
        public async Task<ResourceValidationResult> ValidateRequestedResourcesAsync(Client client, IEnumerable<string> requestedScopes, IEnumerable<string> requestedResourceIdentifiers)
        {
            var result = new ResourceValidationResult();

            var offlineAccess = requestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);
            if (offlineAccess)
            {
                if (await IsClientAllowedOfflineAccessAsync(client))
                {
                    result.Scopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
                else
                {
                    result.InvalidScopesForClient.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
            }

            if (offlineAccess)
            {
                // filter here so below in our validation loop we're not doing extra checking for offline_access
                requestedScopes = requestedScopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess).ToArray();
            }
            
            var resources = await FindResourcesByScopeAsync(requestedScopes);
            resources.OfflineAccess = offlineAccess;

            foreach (var scope in requestedScopes)
            {
                await ValidateScopeAsync(client, resources, scope, result);
            }

            if (result.InvalidScopes.Count > 0 || result.InvalidScopesForClient.Count > 0)
            {
                if (result.InvalidScopes.Count > 0)
                {
                    _logger.LogError("Invalid scopes: {scopes}", result.InvalidScopes);
                }
                if (result.InvalidScopesForClient.Count > 0)
                {
                    _logger.LogError("Invalid scopes for client id: {clientId}, scopes: {scopes}", client.ClientId, result.InvalidScopesForClient);
                }
                
                result.Resources = null;
                result.Scopes.Clear();
                result.RequiredScopes.Clear();
            }
            else
            {
                result.Resources = resources;
            }

            return result;
        }

        /// <summary>
        /// Loads the scopes from the store.
        /// </summary>
        /// <param name="requestedScopes"></param>
        /// <returns></returns>
        protected virtual async Task<Resources> FindResourcesByScopeAsync(IEnumerable<string> requestedScopes)
        {
            //if (requestedScopes.Contains("payment:"))
            //{
            //    var newScopes = requestedScopes.Where(x => !x.StartsWith("payment:")).ToList();
            //    newScopes.Add("payment");
            //}

            // requestedScopes=["payment:123", "api1", "openid"]
            // client:{allowedScopes="payment"}
            // apiResource {  name="payment",  scopes=[ {name="payment:123", userClaims=['email']} ]}

            var result = await _store.FindEnabledResourcesByScopeAsync(requestedScopes);

            // todo: keep original scope string, formal name in db, bag for data, scope value to emit

            //result.ApiResources.Where(x=>x.Name == "payment")["txId"] = 

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resources"></param>
        /// <param name="scope"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task ValidateScopeAsync(Client client, Resources resources, string scope, ResourceValidationResult result)
        {
            var identity = await FindIdentityResourceAsync(resources, scope);
            if (identity != null)
            {
                if (!await IsClientAllowedIdentityResourceAsync(client, identity))
                {
                    result.InvalidScopesForClient.Add(scope);
                }
                else
                {
                    result.Scopes.Add(scope);
                    if (identity.Required)
                    {
                        result.RequiredScopes.Add(scope);
                    }
                }
            }
            else
            {
                var apiScope = await FindApiScopeAsync(resources, scope);
                if (apiScope != null)
                {
                    if (!await IsClientAllowedApiScopeAsync(client, apiScope))
                    {
                        result.InvalidScopesForClient.Add(scope);
                    }
                    else
                    {
                        result.Scopes.Add(scope);
                        if (apiScope.Required)
                        {
                            result.RequiredScopes.Add(scope);
                        }
                    }
                }
                else
                {
                    result.InvalidScopes.Add(scope);
                }
            }
        }

        /// <summary>
        /// Find the matching IdentityResource.
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected virtual Task<IdentityResource> FindIdentityResourceAsync(Resources resources, string scope)
        {
            var identityResource = resources.IdentityResources.FirstOrDefault(x => x.Name == scope);
            return Task.FromResult(identityResource);
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
        /// <param name="resources"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected virtual Task<Scope> FindApiScopeAsync(Resources resources, string scope)
        {
            var apiScope = resources.FindApiScope(scope);
            return Task.FromResult(apiScope);
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

        /// <inheritdoc/>
        public Task<ResourceValidationResult> FilterResourcesAsync(ResourceValidationResult resourceValidationResult, IEnumerable<string> scopes)
        {
            var result = new ResourceValidationResult { 
                Resources = resourceValidationResult.Resources.Filter(scopes),
                Scopes = scopes.ToList()
            };
            return Task.FromResult(result);
        }
    }
}