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
                    result.Resources.OfflineAccess = true;
                    result.ParsedScopes.Add(new ParsedScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess));
                }
                else
                {
                    result.InvalidScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
            }

            // filter here so below in our store call and in the validation loop we're not doing extra checking for offline_access
            var requestedScopes = offlineAccess ?
                request.ScopeValues.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess).ToArray() : 
                request.ScopeValues;

            var parsedScopes = ParseRequestedScopes(requestedScopes);
            var scopeNames = parsedScopes.Select(x => x.Name).Distinct();

            var resourcesFromStore = await _store.FindEnabledResourcesByScopeAsync(scopeNames);
            resourcesFromStore.OfflineAccess = offlineAccess;

            foreach (var scope in parsedScopes)
            {
                await ValidateScopeAsync(request.Client, resourcesFromStore, scope, result);
            }

            if (result.InvalidScopes.Count > 0)
            {
                result.Resources.IdentityResources.Clear();
                result.Resources.ApiResources.Clear();
                result.Resources.Scopes.Clear();
                result.ParsedScopes.Clear();
            }

            return result;
        }

        /// <summary>
        /// Parses the requested scopes.
        /// </summary>
        /// <param name="scopeValues"></param>
        /// <returns></returns>
        protected IEnumerable<ParsedScopeValue> ParseRequestedScopes(IEnumerable<string> scopeValues)
        {
            //if (requestedScopes.Contains("payment:"))
            //{
            //    var newScopes = requestedScopes.Where(x => !x.StartsWith("payment:")).ToList();
            //    newScopes.Add("payment");
            //}

            // requestedScopes=["payment:123", "api1", "openid"]
            // client:{allowedScopes="payment"}
            // apiResource {  name="payment",  scopes=[ {name="payment:123", userClaims=['email']} ]}            
            // todo: keep original scope string, formal name in db, bag for data, scope value to emit

            //result.ApiResources.Where(x=>x.Name == "payment")["txId"] = 

            return scopeValues.Select(x => new ParsedScopeValue(x));
        }

        /// <summary>
        /// Validates that the requested scopes is contained in the store, and the client is allowed to request it.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resourcesFromStore"></param>
        /// <param name="requestedScope"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task ValidateScopeAsync(
            Client client, 
            Resources resourcesFromStore, 
            ParsedScopeValue requestedScope, 
            ResourceValidationResult result)
        {
            var identity = resourcesFromStore.FindIdentityResourcesByScope(requestedScope.Name);
            if (identity != null)
            {
                if (!await IsClientAllowedIdentityResourceAsync(client, identity))
                {
                    result.InvalidScopes.Add(requestedScope.Value);
                }
                else
                {
                    result.ParsedScopes.Add(requestedScope);
                    result.Resources.IdentityResources.Add(identity);
                }
            }
            else
            {
                var apiScope = resourcesFromStore.FindScope(requestedScope.Name);
                if (apiScope != null)
                {
                    if (!await IsClientAllowedApiScopeAsync(client, apiScope))
                    {
                        result.InvalidScopes.Add(requestedScope.Value);
                    }
                    else
                    {
                        result.ParsedScopes.Add(requestedScope);
                        result.Resources.Scopes.Add(apiScope);

                        var apis = resourcesFromStore.FindApiResourcesByScope(apiScope.Name);
                        foreach(var api in apis)
                        {
                            result.Resources.ApiResources.Add(api);
                        }
                    }
                }
                else
                {
                    _logger.LogError("Scope {scope} not found in store.", requestedScope.Name);
                    result.InvalidScopes.Add(requestedScope.Value);
                }
            }
        }

        /// <summary>
        /// Determines if client is allowed access to the identity scope.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedIdentityResourceAsync(Client client, IdentityResource identity)
        {
            var allowed = client.AllowedScopes.Contains(identity.Name);
            if (!allowed)
            {
                _logger.LogError("Client {client} is now allowed access to scope {scope}.", client.ClientId, identity.Name);
            }
            return Task.FromResult(allowed);
        }

        /// <summary>
        /// Determines if client is allowed access to the API scope.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apiScope"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedApiScopeAsync(Client client, Scope apiScope)
        {
            var allowed = client.AllowedScopes.Contains(apiScope.Name);
            if (!allowed)
            {
                _logger.LogError("Client {client} is now allowed access to scope {scope}.", client.ClientId, apiScope.Name);
            }
            return Task.FromResult(allowed);
        }

        /// <summary>
        /// Validates if the client is allowed offline_access.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedOfflineAccessAsync(Client client)
        {
            var allowed = client.AllowOfflineAccess;
            if (!allowed)
            {
                _logger.LogError("Client {client} is now allowed access to scope offline_access (via AllowOfflineAccess setting).", client.ClientId);
            }
            return Task.FromResult(allowed);
        }
    }
}