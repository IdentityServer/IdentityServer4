// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates requested resources (scopes and resource indicators)
    /// </summary>
    public interface IResourceValidator
    {
        /// <summary>
        /// Parses the requested scopes.
        /// </summary>
        Task<IEnumerable<ParsedScopeValue>> ParseRequestedScopesAsync(IEnumerable<string> scopeValues);
        
        /// <summary>
        /// Validates the requested resources for the client.
        /// </summary>
        Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request);
    }
}
