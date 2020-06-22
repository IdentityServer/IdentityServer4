// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// Interface for discovery endpoint response generator
    /// </summary>
    public interface IDiscoveryResponseGenerator
    {
        /// <summary>
        /// Creates the discovery document.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="issuerUri">The issuer URI.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        Task<Dictionary<string, object>> CreateDiscoveryDocumentAsync(string baseUrl, string issuerUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the JWK document.
        /// </summary>
        Task<IEnumerable<JsonWebKey>> CreateJwkDocumentAsync();
    }
}