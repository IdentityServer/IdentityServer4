// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Interface for the key material service
    /// </summary>
    public interface IKeyMaterialService
    {
        /// <summary>
        /// Gets all validation keys.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the signing credentials.
        /// </summary>
        /// <param name="allowedAlgorithms">Collection of algorithms used to filter the server supported algorithms. 
        /// A value of null or empty indicates that the server default should be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all signing credentials.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync(CancellationToken cancellationToken = default);
    }
}