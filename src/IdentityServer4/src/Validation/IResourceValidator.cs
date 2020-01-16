// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
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
}