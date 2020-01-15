// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates scopes
    /// </summary>
    public class ScopeValidator__
    {
        private readonly ILogger _logger;
        private readonly IResourceStore _store;

        // todo: idea: scope context? requested scopes, granted scopes, original scopes (for RT usage?)
        // maybe this implies the presence of an authorize context which is stored and loaded in RT usage
        // then RT requested scopes can be compared/subset checked against authorize reqquest's granted scopes?
        // also, should a request context be diff than a persisted context? need to look into how currently we separate those.

        // todo: scope validator should be separate service in DI
        // todo: requested and granted scopes should be state on validated request (as List<string>).
        // and those schould have strong semantics -- null vs. empty (null means not yet determined, empty means none granted)
        // i would want those lists to be able to be manipulated if custom stuff needs to change what's happening.

        // also, maybe we need something that will return the Resource from the store based on the requested/granted scope list of strings.
        // this is needed because we will persist the requested/granted scopes arrays, but not the full resource object model (in the code/RT/Ref persisted grants).
        // also, that will support custom/dynamic scopes where some need to be honored and other ignored.
        // perhaps the default impl has API to return the tuple(valid, invalid) lists? and then custom can rearrange these?
        // need to know to either ignore or error invalid or unknown scopes? OIDC says SHOULD ignore unrecognized scopes (as opposed to error)

        // todo: replacement will be the "resource validator" to also handle resource indicators
        // as each API will need to prolly accept list of resource indicators and scopes

        // todo: consider requested scopes, scopes to consent, scopes granted

        /// <summary>
        /// Gets a value indicating whether this instance contains identity scopes
        /// </summary>
        /// <value>
        ///   <c>true</c> if it contains identity scopes; otherwise, <c>false</c>.
        /// </value>
        // todo: either on scope context or extension method on list of resources
        public bool ContainsOpenIdScopes => GrantedResources.IdentityResources.Any();

        /// <summary>
        /// Gets a value indicating whether this instance contains API scopes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it contains API scopes; otherwise, <c>false</c>.
        /// </value>
        // todo: either on scope context or extension method on list of resources
        public bool ContainsApiResourceScopes => GrantedResources.ApiResources.Any();

        /// <summary>
        /// Gets a value indicating whether this instance contains the offline access scope.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it contains the offline access scope; otherwise, <c>false</c>.
        /// </value>
        // todo: either on scope context or extension method on list of resources
        public bool ContainsOfflineAccessScope => GrantedResources.OfflineAccess;

        /// <summary>
        /// Gets the requested resources.
        /// </summary>
        /// <value>
        /// The requested resources.
        /// </value>
        // todo: mayve move to scope context or some sort
        public Resources RequestedResources { get; internal set; } = new Resources();

        /// <summary>
        /// Gets the granted resources.
        /// </summary>
        /// <value>
        /// The granted resources.
        /// </value>
        public Resources GrantedResources { get; internal set; } = new Resources();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeValidator"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="logger">The logger.</param>
        //public ScopeValidator(IResourceStore store, ILogger<ScopeValidator> logger)
        //{
        //    _logger = logger;
        //    _store = store;
        //}

        /// <summary>
        /// Validates the required scopes.
        /// </summary>
        /// <param name="consentedScopes">The consented scopes.</param>
        /// <returns></returns>
        // todo: rework for semantics AreAllRequiredScopedConsented/Granted?
        // meant for UI interaction service.
        public bool ValidateRequiredScopes(IEnumerable<string> consentedScopes)
        {
            var identity = RequestedResources.IdentityResources.Where(x => x.Required).Select(x => x.Name);
            var apiQuery = from api in RequestedResources.ApiResources
                           where api.Scopes != null
                           from scope in api.Scopes
                           where scope.Required
                           select scope.Name;

            var requiredScopes = identity.Union(apiQuery);
            return requiredScopes.All(x => consentedScopes.Contains(x));
        }

        /// <summary>
        /// Sets the consented scopes.
        /// </summary>
        /// <param name="consentedScopes">The consented scopes.</param>
        public void SetConsentedScopes(IEnumerable<string> consentedScopes)
        {
            consentedScopes = consentedScopes ?? Enumerable.Empty<string>();

            var offline = consentedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);
            if (offline)
            {
                consentedScopes = consentedScopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
            }

            var identityToKeep = GrantedResources.IdentityResources.Where(x => x.Required || consentedScopes.Contains(x.Name));
            var apisToKeep = from api in GrantedResources.ApiResources
                             where api.Scopes != null
                             let scopesToKeep = (from scope in api.Scopes
                                                 where scope.Required == true || consentedScopes.Contains(scope.Name)
                                                 select scope)
                             where scopesToKeep.Any()
                             select api.CloneWithScopes(scopesToKeep);

            GrantedResources = new Resources(identityToKeep, apisToKeep)
            {
                OfflineAccess = offline
            };
        }

        /// <summary>
        /// Valides given scopes
        /// </summary>
        /// <param name="requestedScopes">The requested scopes.</param>
        /// <param name="filterIdentityScopes">if set to <c>true</c> [filter identity scopes].</param>
        /// <returns></returns>
        // todo: semantics: are these scopes valid in the system
        // called from authZ, device, and token request validators
        public async Task<bool> AreScopesValidAsync(IEnumerable<string> requestedScopes, bool filterIdentityScopes = false)
        {
            if (requestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess))
            {
                GrantedResources.OfflineAccess = true;
                requestedScopes = requestedScopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess).ToArray();

                if (!requestedScopes.Any())
                {
                    _logger.LogError("No identity or API scopes requested");
                    return false;
                }
            }

            var resources = await _store.FindResourcesByScopeAsync(requestedScopes);

            foreach (var requestedScope in requestedScopes)
            {
                var identity = resources.IdentityResources.FirstOrDefault(x => x.Name == requestedScope);
                if (identity != null)
                {
                    if (identity.Enabled == false)
                    {
                        _logger.LogError("Scope disabled: {requestedScope}", requestedScope);
                        return false;
                    }

                    if (!filterIdentityScopes)
                    {
                        GrantedResources.IdentityResources.Add(identity);
                    }
                }
                else
                {
                    var api = resources.FindApiResourceByScope(requestedScope);
                    if (api == null)
                    {
                        _logger.LogError("Invalid scope: {requestedScope}", requestedScope);
                        return false;
                    }

                    if (api.Enabled == false)
                    {
                        _logger.LogError("API {api} that contains scope is disabled: {requestedScope}", api.Name, requestedScope);
                        return false;
                    }

                    var scope = api.FindApiScope(requestedScope);

                    if (scope == null)
                    {
                        _logger.LogError("Invalid scope: {requestedScope}", requestedScope);
                        return false;
                    }

                    // see if we already have this API in our list
                    var existingApi = GrantedResources.ApiResources.FirstOrDefault(x => x.Name == api.Name);
                    if (existingApi != null)
                    {
                        existingApi.Scopes.Add(scope);
                    }
                    else
                    {
                        GrantedResources.ApiResources.Add(api.CloneWithScopes(new[] { scope }));
                    }
                }
            }

            RequestedResources = new Resources(GrantedResources.IdentityResources, GrantedResources.ApiResources)
            {
                OfflineAccess = GrantedResources.OfflineAccess
            };

            return true;
        }

        /// <summary>
        /// Checks is scopes are allowed.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestedScopes">The requested scopes.</param>
        /// <returns></returns>
        // todo: semantics: are these scopes alloed for this client
        // called from authZ, device, and token request validators
        public async Task<bool> AreScopesAllowedAsync(Client client, IEnumerable<string> requestedScopes)
        {
            if (requestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess))
            {
                if (client.AllowOfflineAccess == false)
                {
                    _logger.LogError("offline_access is not allowed for this client: {client}", client.ClientId);
                    return false;
                }
                requestedScopes = requestedScopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess).ToArray();
            }

            var resources = await _store.FindEnabledResourcesByScopeAsync(requestedScopes);

            foreach (var scope in requestedScopes)
            {
                var identity = resources.IdentityResources.FirstOrDefault(x => x.Name == scope);
                if (identity != null)
                {
                    if (!client.AllowedScopes.Contains(scope))
                    {
                        _logger.LogError("Requested scope not allowed: {scope}", scope);
                        return false;
                    }
                }
                else
                {
                    var api = resources.FindApiScope(scope);
                    if (api == null || !client.AllowedScopes.Contains(scope))
                    {
                        _logger.LogError("Requested scope not allowed: {scope}", scope);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the response type is valid.
        /// </summary>
        /// <param name="responseType">Type of the response.</param>
        /// <returns>
        ///   <c>true</c> if the response type is valid; otherwise, <c>false</c>.
        /// </returns>
        // todo: maybe move to authZ request validator? it's a authZ requrest specific thing.
        public bool IsResponseTypeValid(string responseType)
        {
            var requirement = Constants.ResponseTypeToScopeRequirement[responseType];

            switch (requirement)
            {
                case Constants.ScopeRequirement.Identity:
                    if (!ContainsOpenIdScopes)
                    {
                        _logger.LogError("Requests for id_token response type must include identity scopes");
                        return false;
                    }
                    break;
                case Constants.ScopeRequirement.IdentityOnly:
                    if (!ContainsOpenIdScopes || ContainsApiResourceScopes)
                    {
                        _logger.LogError("Requests for id_token response type only must not include resource scopes");
                        return false;
                    }
                    break;
                case Constants.ScopeRequirement.ResourceOnly:
                    if (ContainsOpenIdScopes || !ContainsApiResourceScopes)
                    {
                        _logger.LogError("Requests for token response type only must include resource scopes, but no identity scopes.");
                        return false;
                    }
                    break;
            }

            return true;
        }
    }
}