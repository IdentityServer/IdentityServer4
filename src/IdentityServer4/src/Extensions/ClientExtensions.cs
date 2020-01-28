// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Extension methods for client.
    /// </summary>
    public static class ClientExtensions
    {
        /// <summary>
        /// Returns true if the client is an implicit-only client.
        /// </summary>
        public static bool IsImplicitOnly(this Client client)
        {
            return client != null &&
                client.AllowedGrantTypes != null &&
                client.AllowedGrantTypes.Count == 1 &&
                client.AllowedGrantTypes.First() == GrantType.Implicit;
        }

        /// <summary>
        /// Constructs a list of SecurityKey from a Secret collection
        /// </summary>
        /// <param name="secrets">The secrets</param>
        /// <returns></returns>
        public static Task<List<SecurityKey>> GetKeysAsync(this IEnumerable<Secret> secrets)
        {
            var secretList = secrets.ToList().AsReadOnly();
            var keys = new List<SecurityKey>();

            var certificates = GetCertificates(secretList)
                                .Select(c => (SecurityKey)new X509SecurityKey(c))
                                .ToList();
            keys.AddRange(certificates);

            var jwks = secretList
                        .Where(s => s.Type == IdentityServerConstants.SecretTypes.JsonWebKey)
                        .Select(s => new Microsoft.IdentityModel.Tokens.JsonWebKey(s.Value))
                        .ToList();
            keys.AddRange(jwks);

            return Task.FromResult(keys);
        }

        private static List<X509Certificate2> GetCertificates(IEnumerable<Secret> secrets)
        {
            return secrets
                .Where(s => s.Type == IdentityServerConstants.SecretTypes.X509CertificateBase64)
                .Select(s => new X509Certificate2(Convert.FromBase64String(s.Value)))
                .Where(c => c != null)
                .ToList();
        }
    }
}