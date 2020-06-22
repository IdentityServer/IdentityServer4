// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates requested resources (scopes and resource indicators)
    /// </summary>
    public interface IResourceValidator
    {
        // todo: should this be used anywhere we re-create tokens? do we need to re-run scope validation?

        /// <summary>
        /// Validates the requested resources for the client.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request, CancellationToken cancellationToken = default);
    }
}
