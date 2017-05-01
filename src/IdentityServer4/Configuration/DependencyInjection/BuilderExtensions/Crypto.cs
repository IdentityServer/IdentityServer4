// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Serialization;
using CryptoRandom = IdentityModel.CryptoRandom;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering crypto services
    /// </summary>
    public static class IdentityServerBuilderExtensionsCrypto
    {
        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="credential">The credential.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, SigningCredentials credential)
        {
            // todo
            if (!(credential.Key is AsymmetricSecurityKey
                || (credential.Key is JsonWebKey && ((JsonWebKey)credential.Key).HasPrivateKey)))
            //&& !credential.Key.IsSupportedAlgorithm(SecurityAlgorithms.RsaSha256Signature))
            {
                throw new InvalidOperationException("Signing key is not asymmetric");
            }
            builder.Services.AddSingleton<ISigningCredentialStore>(new DefaultSigningCredentialsStore(credential));
            builder.Services.AddSingleton<IValidationKeysStore>(new DefaultValidationKeysStore(new[] { credential.Key }));

            return builder;
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="certificate">The certificate.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">X509 certificate does not have a private key.</exception>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, X509Certificate2 certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("X509 certificate does not have a private key.");
            }

            var credential = new SigningCredentials(new X509SecurityKey(certificate), "RS256");
            return builder.AddSigningCredential(credential);
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="location">The location.</param>
        /// <param name="nameType">Name parameter can be either a distinguished name or a thumbprint</param>
        /// <exception cref="InvalidOperationException">certificate: '{name}'</exception>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, string name, StoreLocation location = StoreLocation.LocalMachine, NameType nameType = NameType.SubjectDistinguishedName)
        {
            X509Certificate2 certificate = null;

            if (location == StoreLocation.LocalMachine)
            {
                if (nameType == NameType.SubjectDistinguishedName)
                {
                    certificate = X509.LocalMachine.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
                }
                else if (nameType == NameType.Thumbprint)
                {
                    certificate = X509.LocalMachine.My.Thumbprint.Find(name, validOnly: false).FirstOrDefault();
                }
            }
            else
            {
                if (nameType == NameType.SubjectDistinguishedName)
                {
                    certificate = X509.CurrentUser.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
                }
                else if (nameType == NameType.Thumbprint)
                {
                    certificate = X509.CurrentUser.My.Thumbprint.Find(name, validOnly: false).FirstOrDefault();
                }
            }

            if (certificate == null) throw new InvalidOperationException($"certificate: '{name}' not found in certificate store");

            return builder.AddSigningCredential(certificate);
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="rsaKey">The RSA key.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">RSA key does not have a private key.</exception>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, RsaSecurityKey rsaKey)
        {
            if (!rsaKey.HasPrivateKey)
            {
                throw new InvalidOperationException("RSA key does not have a private key.");
            }

            var credential = new SigningCredentials(rsaKey, "RS256");
            return builder.AddSigningCredential(credential);
        }

        /// <summary>
        /// Sets the temporary signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddTemporarySigningCredential(this IIdentityServerBuilder builder)
        {
            var key = CreateRsaSecurityKey();

            return builder.AddSigningCredential(new SigningCredentials(key, "RS256"));
        }

        /// <summary>
        /// Sets the temporary signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDeveloperSigningCredential(this IIdentityServerBuilder builder, string filename = null)
        {
            if (filename == null)
            {
                filename = Path.Combine(Directory.GetCurrentDirectory(), "tempkey.rsa");
            }

            if (File.Exists(filename))
            {
                var keyFile = File.ReadAllText(filename);
                var tempKey = JsonConvert.DeserializeObject<TemporaryRsaKey>(keyFile, new JsonSerializerSettings() { ContractResolver = new RsaKeyContractResolver() });

                return builder.AddSigningCredential(CreateRsaSecurityKey(tempKey.Parameters, tempKey.KeyId));
            }
            else
            {
                var key = CreateRsaSecurityKey();

                RSAParameters parameters;

                if (key.Rsa != null)
                    parameters = key.Rsa.ExportParameters(includePrivateParameters: true);
                else
                    parameters = key.Parameters;

                var tempKey = new TemporaryRsaKey
                {
                    Parameters = parameters,
                    KeyId = key.KeyId
                };

                File.WriteAllText(filename, JsonConvert.SerializeObject(tempKey, new JsonSerializerSettings() { ContractResolver = new RsaKeyContractResolver() }));
                return builder.AddSigningCredential(key);
            }
        }

        /// <summary>
        /// Creates an RSA security key.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static RsaSecurityKey CreateRsaSecurityKey(RSAParameters parameters, string id)
        {
            var key = new RsaSecurityKey(parameters)
            {
                KeyId = id
            };

            return key;
        }

        /// <summary>
        /// Creates a new RSA security key.
        /// </summary>
        /// <returns></returns>
        public static RsaSecurityKey CreateRsaSecurityKey()
        {
            var rsa = RSA.Create();

#if NET452
            if (rsa.KeySize < 2048)
            {
                rsa.Dispose();
                rsa = new RSACryptoServiceProvider(2048);
            }
#endif
            RsaSecurityKey key = null;
#if NET452
            if (rsa is RSACryptoServiceProvider) 
            {
                var parameters = rsa.ExportParameters(includePrivateParameters: true);
                key = new RsaSecurityKey(parameters);
                        
                rsa.Dispose();
            }   
#endif
            if (key == null)
            {
                key = new RsaSecurityKey(rsa);
            }

            key.KeyId = CryptoRandom.CreateUniqueId(16);

            return key;
        }

        /// <summary>
        /// Adds the validation keys.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddValidationKeys(this IIdentityServerBuilder builder, params AsymmetricSecurityKey[] keys)
        {
            builder.Services.AddSingleton<IValidationKeysStore>(new DefaultValidationKeysStore(keys));

            return builder;
        }

        // used for serialization to temporary RSA key
        private class TemporaryRsaKey
        {
            public string KeyId { get; set; }
            public RSAParameters Parameters { get; set; }
        }

        private class RsaKeyContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                property.Ignored = false;

                return property;
            }
        }
    }

    /// <summary>
    /// Describes the string so we know what to search for in certificate store
    /// </summary>
    public enum NameType
    {
        /// <summary>
        /// subject distinguished name
        /// </summary>
        SubjectDistinguishedName,

        /// <summary>
        /// thumbprint
        /// </summary>
        Thumbprint
    }
}