/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
 using System.Collections.Generic;

namespace IdentityServer4.Core.Configuration
{
    /// <summary>
    /// Options class to configure discovery endpoint
    /// </summary>
    public class DiscoveryOptions
    {
        /// <summary>
        /// Show endpoints
        /// </summary>
        public bool ShowEndpoints { get; set; }

        /// <summary>
        /// Show signing keys
        /// </summary>
        public bool ShowKeySet { get; set; }

        /// <summary>
        /// Show identity scopes
        /// </summary>
        public bool ShowIdentityScopes { get; set; }

        /// <summary>
        /// Show resource scopes
        /// </summary>
        public bool ShowResourceScopes { get; set; }

        /// <summary>
        /// Show identity claims
        /// </summary>
        public bool ShowClaims { get; set; }

        /// <summary>
        /// Show response types
        /// </summary>
        public bool ShowResponseTypes { get; set; }

        /// <summary>
        /// Show response modes
        /// </summary>
        public bool ShowResponseModes { get; set; }

        /// <summary>
        /// Show standard grant types
        /// </summary>
        public bool ShowGrantTypes { get; set; }

        /// <summary>
        /// Show custom grant types
        /// </summary>
        public bool ShowCustomGrantTypes { get; set; }

        /// <summary>
        /// Show token endpoint authentication methods
        /// </summary>
        public bool ShowTokenEndpointAuthenticationMethods { get; set; }

        /// <summary>
        /// Adds custom entries to the discovery document
        /// </summary>
        public Dictionary<string, object> CustomEntries { get; set; }

        /// <summary>
        /// Initializes with default values
        /// </summary>
        public DiscoveryOptions()
        {
            ShowEndpoints = true;
            ShowKeySet = true;
            ShowIdentityScopes = true;
            ShowResourceScopes = true;
            ShowClaims = true;
            ShowResponseTypes = true;
            ShowResponseModes = true;
            ShowGrantTypes = true;
            ShowCustomGrantTypes = true;
            ShowTokenEndpointAuthenticationMethods = true;
            CustomEntries = new Dictionary<string, object>();
        }
    }
}