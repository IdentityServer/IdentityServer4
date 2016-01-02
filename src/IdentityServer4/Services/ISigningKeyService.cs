// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Service that deals with public and private keys used for token generation and metadata
    /// </summary>
    public interface ISigningKeyService
    {
        /// <summary>
        /// Retrieves the primary signing key
        /// </summary>
        /// <returns>Signing key</returns>
        Task<X509Certificate2> GetSigningKeyAsync();

        /// <summary>
        /// Retrieves all public keys that can be used to validate tokens
        /// </summary>
        /// <returns>Public keys</returns>
        Task<IEnumerable<X509Certificate2>> GetValidationKeysAsync();

        /// <summary>
        /// Calculates the key id for a given x509 certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        Task<string> GetKidAsync(X509Certificate2 certificate);
    }
}