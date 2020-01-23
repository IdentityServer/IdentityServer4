// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Validation
{
    public class ResourceValidationRequest
    {
        public Client Client { get; set; }
        public IEnumerable<string> ScopeValues { get; set; }
        public IEnumerable<string> ResourceIdentifiers { get; set; }
    }

    /// <summary>
    /// Result of validation of requested scopes and resource indicators.
    /// </summary>
    public class ResourceValidationResult
    {
        /// <summary>
        /// Indicates if the result was successful.
        /// </summary>
        public bool Succeeded => IdentityResources.Any() || ApiResources.Any() || Scopes.Any();

        // todo: brock change these to APIs to add, rather then iterateable collections
        public ICollection<ScopeValue> Scopes { get; set; } = new HashSet<ScopeValue>();
        public ICollection<ApiResource> ApiResources { get; set; } = new HashSet<ApiResource>();
        public ICollection<IdentityResource> IdentityResources { get; set; } = new HashSet<IdentityResource>();

        ///// <summary>
        ///// The valid scope values.
        ///// </summary>
        //public ICollection<ValidatedScope> ValidScopes { get; set; } = new HashSet<ValidatedScope>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ScopeValues => IdentityResources.Select(x => x.Name).Union(Scopes.Select(x => x.Value)).ToArray();

        /// <summary>
        /// 
        /// </summary>
        //public IEnumerable<string> RequiredScopeValues => ValidScopes.Where(x => x.Required).Select(x => x.Value).ToArray();

        /// <summary>
        /// 
        /// </summary>
        //public IEnumerable<IdentityResource> IdentityResources => ValidScopes.Where(x => x.IdentityResource != null).Select(x => x.IdentityResource);

        /// <summary>
        /// 
        /// </summary>
        //public IEnumerable<ApiResource> ApiResources
        //{
        //    get
        //    {
        //        var apis = new Dictionary<string, ApiResource>();

        //        foreach(var validScope in ValidScopes)
        //        {
        //            if (validScope.Apis != null)
        //            {
        //                foreach(var api in validScope.Apis)
        //                {
        //                    if (!apis.ContainsKey(api.Name))
        //                    {
        //                        apis[api.Name] = api.CloneWithScopes(new[] { validScope.Scope });
        //                    }
        //                    else
        //                    {
        //                        var apiResource = apis[api.Name];
        //                        if (apiResource.FindApiScope(validScope.Scope.Name) == null)
        //                        {
        //                            apiResource.Scopes.Add(validScope.Scope);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return apis.Select(x => x.Value).ToArray();
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        //public IEnumerable<Scope> Scopes => ValidScopes.Where(x => x.Scope != null).Select(x => x.Scope);

        /// <summary>
        /// 
        /// </summary>
        public Resources Resources => new Resources(IdentityResources, ApiResources, Scopes.Where(x => x.Scope != null).Select(x => x.Scope)) { OfflineAccess = ScopeValues.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) };

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
            return Resources.Filter(scopeValues).ToResourceValidationResult();
        }
    }

    public class ScopeValue
    {
        public ScopeValue(string value)
        {
            Value = value;
        }

        public ScopeValue(string value, Scope scope)
            : this(value)
        {
            Scope = scope;
        }
        
        public ScopeValue(Scope scope)
            : this(scope.Name, scope)
        {
        }

        public string Value { get; set; }
        public Scope Scope { get; set; }
    }


    /// <summary>
    /// Models the validated scope value.
    /// </summary>
    //public class ValidatedScope
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="value"></param>
    //    public ValidatedScope(string value)
    //    {
    //        Value = value ?? throw new ArgumentNullException(nameof(value));
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <param name="identityResource"></param>
    //    public ValidatedScope(string value, IdentityResource identityResource)
    //        : this(value)
    //    {
    //        IdentityResource = identityResource ?? throw new ArgumentNullException(nameof(identityResource));
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <param name="apis"></param>
    //    /// <param name="scope"></param>
    //    public ValidatedScope(string value, IEnumerable<ApiResource> apis, Scope scope)
    //        : this(value)
    //    {
    //        Apis = apis ?? throw new ArgumentNullException(nameof(apis));
    //        Scope = scope ?? throw new ArgumentNullException(nameof(scope));

    //        if (!apis.Any()) throw new ArgumentException("API resource collection is empty.", nameof(apis));
    //        if (!apis.All(x => x.Scopes.Any(y => y.Name == scope.Name))) throw new ArgumentException($"Not all API resources contain the scope {scope.Name}", nameof(apis));
    //    }

    //    /// <summary>
    //    /// The value of the scope.
    //    /// </summary>
    //    public string Value { get; set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public bool Required => IdentityResource?.Required == true || Scope?.Required == true;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public IdentityResource IdentityResource { get; set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public IEnumerable<ApiResource> Apis { get; set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Scope Scope { get; set; }
    //}
}
