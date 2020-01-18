// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Result of validation of requested scopes and resource indicators.
    /// </summary>
    public class ResourceValidationResult
    {
        /// <summary>
        /// Indicates if the result was successful.
        /// </summary>
        public bool Succeeded => ValidScopes?.Any() == true;

        /// <summary>
        /// The valid scope values.
        /// </summary>
        public ICollection<ValidatedScope> ValidScopes { get; set; } = new HashSet<ValidatedScope>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ScopeValues => ValidScopes.Select(x => x.Value).ToArray();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> RequiredScopeValues => ValidScopes.Where(x => x.Required).Select(x => x.Value).ToArray();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IdentityResource> IdentityResources => ValidScopes.Where(x => x.IdentityResource != null).Select(x => x.IdentityResource);
        
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ApiResource> ApiResources
        {
            get
            {
                var validScopes = ValidScopes.Where(x => x.Api != null);
                var groupedScopes = validScopes.GroupBy(x => x.Api.Name);
                var apiResources = groupedScopes.Select(x => x.First().Api.CloneWithScopes(x.Select(y => y.Scope)));
                return apiResources;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Scope> Scopes => ValidScopes.Where(x => x.Scope != null).Select(x => x.Scope);

        /// <summary>
        /// 
        /// </summary>
        public Resources Resources => new Resources(IdentityResources, ApiResources) { OfflineAccess = ScopeValues.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) };

        /// <summary>
        /// The requested scopes that are invalid.
        /// </summary>
        public ICollection<string> InvalidScopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// The requested scopes that are not allowed for the client.
        /// </summary>
        public ICollection<string> InvalidScopesForClient { get; set; } = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeValues"></param>
        /// <returns></returns>
        public ResourceValidationResult Filter(IEnumerable<string> scopeValues)
        {
            var scopes = ValidScopes.Where(x => scopeValues.Contains(x.Value));
            return new ResourceValidationResult {
                ValidScopes = scopes.ToList(),
            };
        }
    }

    /// <summary>
    /// Models the validated scope value.
    /// </summary>
    public class ValidatedScope
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public ValidatedScope(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="identityResource"></param>
        public ValidatedScope(string value, IdentityResource identityResource)
            : this(value)
        {
            IdentityResource = identityResource ?? throw new ArgumentNullException(nameof(identityResource));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="api"></param>
        /// <param name="scope"></param>
        public ValidatedScope(string value, ApiResource api, Scope scope)
            : this(value)
        {
            Api = api ?? throw new ArgumentNullException(nameof(api));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            
            if (Api.FindApiScope(scope.Name) == null) throw new ArgumentException($"Scope '{scope.Name}' is not a scope of ApiResource '{api.Name}'.");
        }

        /// <summary>
        /// The value of the scope.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Required => IdentityResource?.Required == true || Scope?.Required == true;

        /// <summary>
        /// 
        /// </summary>
        public IdentityResource IdentityResource { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public ApiResource Api { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Scope Scope { get; set; }
    }
}
