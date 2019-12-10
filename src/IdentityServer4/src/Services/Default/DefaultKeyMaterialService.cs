// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using System.Linq;
using System;

namespace IdentityServer4.Services
{
    /// <summary>
    /// The default key material service
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IKeyMaterialService" />
    public class DefaultKeyMaterialService : IKeyMaterialService
    {
        private readonly IEnumerable<ISigningCredentialStore> _signingCredentialStores;
        private readonly IEnumerable<IValidationKeysStore> _validationKeysStores;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultKeyMaterialService"/> class.
        /// </summary>
        /// <param name="validationKeysStores">The validation keys stores.</param>
        /// <param name="signingCredentialStores">The signing credential store.</param>
        public DefaultKeyMaterialService(IEnumerable<IValidationKeysStore> validationKeysStores, IEnumerable<ISigningCredentialStore> signingCredentialStores = null)
        {
            _signingCredentialStores = signingCredentialStores;
            _validationKeysStores = validationKeysStores;
        }

        /// <summary>
        /// Gets the default signing credentials.
        /// </summary>
        /// <returns></returns>
        public async Task<SigningCredentials> GetSigningCredentialsAsync(string algorithm = null)
        {
            if (_signingCredentialStores != null)
            {
                if (algorithm is null)
                {
                    return await _signingCredentialStores.First().GetSigningCredentialsAsync();
                }
                else
                {
                    var credential = (await GetAllSigningCredentials()).FirstOrDefault(c => c.Algorithm.Equals(algorithm));
                    if (credential is null)
                    {
                        throw new InvalidOperationException($"No signing credential for algorithm {algorithm} registered.");
                    }

                    return credential;
                }
            }

            return null;
        }

        public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentials()
        {
            var credentials = new List<SigningCredentials>();

            if (_signingCredentialStores != null)
            {
                foreach (var store in _signingCredentialStores)
                {
                    credentials.Add(await store.GetSigningCredentialsAsync());
                }
            }

            return credentials;
        }

        /// <summary>
        /// Gets all validation keys.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var keys = new List<SecurityKeyInfo>();

            foreach (var store in _validationKeysStores)
            {
                keys.AddRange(await store.GetValidationKeysAsync());
            }

            return keys;
        }
    }
}