// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models are resource (either identity resource or web api resource)
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// Indicates if scope is enabled and can be requested. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Name of the scope. This is the value a client will use to request the scope.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Display name. This value will be used e.g. on the consent screen.
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description. This value will be used e.g. on the consent screen.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
        /// </summary>
        public bool Emphasize { get; set; } = false;

        /// <summary>
        /// Specifies whether this scope is about identity information from the userinfo endpoint, or a resource (e.g. a Web API). Defaults to Resource.
        /// </summary>
        public ScopeType Type { get; set; } = ScopeType.Resource;

        /// <summary>
        /// List of user claims that should be included in the identity (identity scope) or access token (resource scope). 
        /// </summary>
        public ICollection<ScopeClaim> Claims { get; set; } = new HashSet<ScopeClaim>();

        /// <summary>
        /// If enabled, all claims for the user will be included in the token. Defaults to false.
        /// </summary>
        public bool IncludeAllClaimsForUser { get; set; } = false;
        
        /// <summary>
        /// Rule for determining which claims should be included in the token (this is implementation specific)
        /// </summary>
        public string ClaimsRule { get; set; }

        /// <summary>
        /// Specifies whether this scope is shown in the discovery document. Defaults to true.
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// Gets or sets the scope secrets.
        /// </summary>
        /// <value>
        /// The scope secrets.
        /// </value>
        public ICollection<Secret> ScopeSecrets { get; set; } = new HashSet<Secret>();

        /// <summary>
        /// Specifies whether this scope is allowed to see other scopes when using the introspection endpoint
        /// </summary>
        public bool AllowUnrestrictedIntrospection { get; set; } = false;
    }
}