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
    /// Validates requested resources (scopes and resource identifiers)
    /// </summary>
    public interface IResourceValidator
    {
        /// <summary>
        /// Validates the requested resources for the client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="requestedScopes"></param>
        /// <param name="requestedResourceIdentifiers"></param>
        /// <returns></returns>
        Task<ResourceValidationResult> ValidateRequestedResources(Client client, IEnumerable<string> requestedScopes, IEnumerable<string> requestedResourceIdentifiers);
    }

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




        /// <summary>
        /// Validates the requested resources for the client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="requestedScopes"></param>
        /// <param name="requestedResourceIdentifiers"></param>
        /// <returns></returns>
        public async Task<ResourceValidationResult> ValidateRequestedResources(Client client, IEnumerable<string> requestedScopes, IEnumerable<string> requestedResourceIdentifiers)
        {
            var result = new ResourceValidationResult();

            var offlineAccess = requestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);
            if (offlineAccess)
            {
                if (client.AllowOfflineAccess == false)
                {
                    result.InvalidScopesForClient.Add("offline_access");
                }
                else
                {
                    requestedScopes = requestedScopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess).ToArray();
                }
            }

            var resources = await _store.FindEnabledResourcesByScopeAsync(requestedScopes);

            foreach (var scope in requestedScopes)
            {
                var identity = resources.IdentityResources.FirstOrDefault(x => x.Name == scope);
                if (identity != null)
                {
                    if (!client.AllowedScopes.Contains(scope))
                    {
                        result.InvalidScopesForClient.Add(scope);
                    }
                }
                else
                {
                    var api = resources.FindApiScope(scope);
                    if (api != null)
                    {
                        if (!client.AllowedScopes.Contains(scope))
                        {
                            result.InvalidScopesForClient.Add(scope);
                        }
                    }
                    else
                    {
                        result.InvalidScopes.Add(scope);
                    }
                }
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
            }
            else
            {
                resources.OfflineAccess = offlineAccess;
                result.ValidatedResources = resources;
            }
            
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResourceValidationResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded => ValidatedResources != null;

        /// <summary>
        /// 
        /// </summary>
        public Resources ValidatedResources { get; set; }

        /// <summary>
        /// Collection of scopes that are invalid.
        /// </summary>
        public ICollection<string> InvalidScopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Collection of scopes that are invalid for the client.
        /// </summary>
        public ICollection<string> InvalidScopesForClient { get; set; } = new HashSet<string>();
    }
}