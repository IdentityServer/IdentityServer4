// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Configuration;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.Default
{
    /// <summary>
    /// Default signing key service based on IdentityServerOptions
    /// </summary>
    public class DefaultSigningKeyService : ISigningKeyService
    {
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Initializes the services with identity server options
        /// </summary>
        /// <param name="options"></param>
        public DefaultSigningKeyService(IdentityServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Calculates the key id for a given x509 certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns>kid</returns>
        public Task<string> GetKidAsync(X509Certificate2 certificate)
        {
            return Task.FromResult(Base64Url.Encode(certificate.GetCertHash()));
        }

        /// <summary>
        /// Retrieves all public keys that can be used to validate tokens
        /// </summary>
        /// <returns>x509 certificates</returns>
        public Task<IEnumerable<X509Certificate2>> GetValidationKeysAsync()
        {
            return Task.FromResult(_options.PublicKeysForMetadata);
        }

        /// <summary>
        /// Retrieves the primary signing key
        /// </summary>
        /// <returns>x509 certificate</returns>
        public Task<X509Certificate2> GetSigningKeyAsync()
        {
            return Task.FromResult(_options.SigningCertificate);
        }
    }
}