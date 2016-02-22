// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public class ScopeValidator
    {
        private readonly ILogger _logger;
        
        private readonly IScopeStore _store;

        public bool ContainsOpenIdScopes { get; private set; }
        public bool ContainsResourceScopes { get; private set; }
        public bool ContainsOfflineAccessScope { get; set; }

        public List<Scope> RequestedScopes { get; private set; }
        public List<Scope> GrantedScopes { get; private set; }

        public ScopeValidator(IScopeStore store, ILoggerFactory loggerFactory)
        {
            RequestedScopes = new List<Scope>();
            GrantedScopes = new List<Scope>();

            _logger = loggerFactory.CreateLogger<ScopeValidator>();
            _store = store;
        }

        public bool ValidateRequiredScopes(IEnumerable<string> consentedScopes)
        {
            var requiredScopes = RequestedScopes.Where(x => x.Required).Select(x=>x.Name);
            return requiredScopes.All(x => consentedScopes.Contains(x));
        }

        public void SetConsentedScopes(IEnumerable<string> consentedScopes)
        {
            consentedScopes = consentedScopes ?? Enumerable.Empty<string>();

            GrantedScopes.RemoveAll(scope => !scope.Required && !consentedScopes.Contains(scope.Name));
        }

        public async Task<bool> AreScopesValidAsync(IEnumerable<string> requestedScopes)
        {
            var availableScopes = await _store.FindScopesAsync(requestedScopes);

            foreach (var requestedScope in requestedScopes)
            {
                var scopeDetail = availableScopes.FirstOrDefault(s => s.Name == requestedScope);

                if (scopeDetail == null)
                {
                    _logger.LogError("Invalid scope: {requestedScope}", requestedScope);
                    return false;
                }

                if (scopeDetail.Enabled == false)
                {
                    _logger.LogError("Scope disabled: {requestedScope}", requestedScope);
                    return false;
                }

                if (scopeDetail.Type == ScopeType.Identity)
                {
                    ContainsOpenIdScopes = true;
                }
                else
                {
                    ContainsResourceScopes = true;
                }

                GrantedScopes.Add(scopeDetail);
            }

            if (requestedScopes.Contains(Constants.StandardScopes.OfflineAccess))
            {
                ContainsOfflineAccessScope = true;
            }

            RequestedScopes.AddRange(GrantedScopes);

            return true;
        }

        public bool AreScopesAllowed(Client client, IEnumerable<string> requestedScopes)
        {
            if (client.AllowAccessToAllScopes)
            {
                return true;
            }

            foreach (var scope in requestedScopes)
            {
                if (!client.AllowedScopes.Contains(scope))
                {
                    _logger.LogError("Requested scope not allowed: {scope}", scope);
                    return false;
                }
            }

            return true;
        }

        public bool IsResponseTypeValid(string responseType)
        {
            var requirement = Constants.ResponseTypeToScopeRequirement[responseType];

            // must include identity scopes
            if (requirement == Constants.ScopeRequirement.Identity)
            {
                if (!ContainsOpenIdScopes)
                {
                    _logger.LogError("Requests for id_token response type must include identity scopes");
                    return false;
                }
            }

            // must include identity scopes only
            else if (requirement == Constants.ScopeRequirement.IdentityOnly)
            {
                if (!ContainsOpenIdScopes || ContainsResourceScopes)
                {
                    _logger.LogError("Requests for id_token response type only must not include resource scopes");
                    return false;
                }
            }

            // must include resource scopes only
            else if (requirement == Constants.ScopeRequirement.ResourceOnly)
            {
                if (ContainsOpenIdScopes || !ContainsResourceScopes)
                {
                    _logger.LogError("Requests for token response type only must include resource scopes, but no identity scopes.");
                    return false;
                }
            }

            return true;
        }
    }
}