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
    using IdentityServer4;

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
            // todo dom
            if (!(credential.Key is AsymmetricSecurityKey
                || credential.Key is JsonWebKey && ((JsonWebKey)credential.Key).HasPrivateKey))
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
            var certificate = FindCertificate(name, location, nameType);
            if (certificate == null) throw new InvalidOperationException($"certificate: '{name}' not found in certificate store");

            return builder.AddSigningCredential(certificate);
        }

        /// <summary>
        /// Sets the signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="securityKey">The RSA key.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">RSA key does not have a private key.</exception>
        public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder, AsymmetricSecurityKey securityKey)
        {
            SigningCredentials credential = null;
            if (securityKey.PrivateKeyStatus == PrivateKeyStatus.DoesNotExist)
            {
                throw new InvalidOperationException("RSA key does not have a private key.");
            }

            string algorithm = null;
            if (securityKey is RsaSecurityKey)
            {
                algorithm = IdentityServerConstants.SigningAlgorithms.RS256;
            }
            else if (securityKey is ECDsaSecurityKey ecDsaSecurityKey)
            {
                switch (ecDsaSecurityKey.KeySize)
                {
                    case 256:
                        algorithm = IdentityServerConstants.SigningAlgorithms.ES256;
                        break;
                    case 384:
                        algorithm = IdentityServerConstants.SigningAlgorithms.ES256;
                        break;
                    case 521:
                        algorithm = IdentityServerConstants.SigningAlgorithms.ES256;
                        break;
                }
            }
            else
            {
                throw new ArgumentException("The securityKey argument must either be an instance of RsaSecurityKey or EcDsaSecurityKey.", nameof(securityKey));
            }

            credential = new SigningCredentials(securityKey, algorithm);
            return builder.AddSigningCredential(credential);
        }

        private class DeveloperSigningKeyFileNameExtensions
        {
            public const string Rsa = "rsa";
            public const string EcDsa = "ecdsa";
        }

        /// <summary>
        /// Sets the temporary signing credential.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="persistKey">Specifies if the temporary key should be persisted to disk.</param>
        /// <param name="ecdsa">Specifies if the generated key pair should be ECDSA-based instead of RSA.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDeveloperSigningCredential(this IIdentityServerBuilder builder, bool persistKey = true, string intendedSigningAlgorithm = IdentityServerConstants.SigningAlgorithms.RS256, string filename = null)
        {
            var isEcDsa = intendedSigningAlgorithm != IdentityServerConstants.SigningAlgorithms.RS256;
            if (filename == null)
            {
                var extension = isEcDsa 
                    ? DeveloperSigningKeyFileNameExtensions.EcDsa
                    : DeveloperSigningKeyFileNameExtensions.Rsa;
                filename = Path.Combine(Directory.GetCurrentDirectory(), $"tempkey.{extension}");
            }

            var fileInfo = new FileInfo(filename);

            if (fileInfo.Exists)
            {
                var keyFile = File.ReadAllText(filename);
                if (fileInfo.Extension.ToLowerInvariant() == DeveloperSigningKeyFileNameExtensions.EcDsa)
                {
                    var tempKey = JsonConvert.DeserializeObject<TemporaryEcDsaKey>(keyFile, new JsonSerializerSettings { ContractResolver = new SecurityKeyContractResolver() });
                    return builder.AddSigningCredential(CreateEcDsaSecurityKey(tempKey.Parameters, tempKey.KeyId));
                }
                else // assume RSA
                {
                    var tempKey = JsonConvert.DeserializeObject<TemporaryRsaKey>(keyFile, new JsonSerializerSettings { ContractResolver = new SecurityKeyContractResolver() });
                    return builder.AddSigningCredential(CreateRsaSecurityKey(tempKey.Parameters, tempKey.KeyId));
                }
            }

            if (isEcDsa)
            {
                var key = CreateEcDsaSecurityKey();
                var parameters = key.ECDsa.ExportParameters(true);
                var tempKey = new TemporaryEcDsaKey
                {
                    Parameters = parameters,
                    KeyId = key.KeyId
                };
                if (persistKey)
                {
                    File.WriteAllText(filename,
                        JsonConvert.SerializeObject(tempKey,
                            new JsonSerializerSettings { ContractResolver = new SecurityKeyContractResolver() }));
                }
                return builder.AddSigningCredential(key);
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

                if (persistKey)
                {
                    File.WriteAllText(filename,
                        JsonConvert.SerializeObject(tempKey,
                            new JsonSerializerSettings { ContractResolver = new SecurityKeyContractResolver() }));
                }
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
        /// Creates an ECDSA security key.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static ECDsaSecurityKey CreateEcDsaSecurityKey(ECParameters parameters, string id)
        {
            var key = new ECDsaSecurityKey(ECDsa.Create(parameters))
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
            RsaSecurityKey key;

            if (rsa is RSACryptoServiceProvider)
            {
                rsa.Dispose();
                var cng = new RSACng(2048);

                var parameters = cng.ExportParameters(includePrivateParameters: true);
                key = new RsaSecurityKey(parameters);
            }
            else
            {
                rsa.KeySize = 2048;
                key = new RsaSecurityKey(rsa);
            }

            key.KeyId = CryptoRandom.CreateUniqueId(16);
            return key;
        }

        /// <summary>
        /// Creates a new ECDSA security key.
        /// </summary>
        /// <param name="curveName">The elliptic curve domain parameters to use. Defaults to "ECDSA_P256". Other valid parameters are "ECDSA_P384" and "ECDSA_P521"</param>
        /// <returns></returns>
        public static ECDsaSecurityKey CreateEcDsaSecurityKey(string curveName = "ECDSA_P256")
        {
            var curve = ECCurve.CreateFromFriendlyName(curveName);
            var ecdsa = ECDsa.Create(curve);

            var key = new ECDsaSecurityKey(ecdsa)
            {
                KeyId = CryptoRandom.CreateUniqueId(16)
            };
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

        /// <summary>
        /// Adds the validation key.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="certificate">The certificate.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IIdentityServerBuilder AddValidationKey(this IIdentityServerBuilder builder, X509Certificate2 certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            var key = new X509SecurityKey(certificate);
            return builder.AddValidationKeys(key);
        }

        /// <summary>
        /// Adds the validation key from the certificate store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="location">The location.</param>
        /// <param name="nameType">Name parameter can be either a distinguished name or a thumbprint</param>
        public static IIdentityServerBuilder AddValidationKey(this IIdentityServerBuilder builder, string name, StoreLocation location = StoreLocation.LocalMachine, NameType nameType = NameType.SubjectDistinguishedName)
        {
            var certificate = FindCertificate(name, location, nameType);
            if (certificate == null) throw new InvalidOperationException($"certificate: '{name}' not found in certificate store");

            return builder.AddValidationKey(certificate);
        }

        private static X509Certificate2 FindCertificate(string name, StoreLocation location, NameType nameType)
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

            return certificate;
        }

        // used for serialization to temporary security key files
        private class TemporaryRsaKey
        {
            public string KeyId { get; set; }
            public RSAParameters Parameters { get; set; }
        }

        private class TemporaryEcDsaKey
        {
            public string KeyId { get; set; }
            public ECParameters Parameters { get; set; }
        }


        private class SecurityKeyContractResolver : DefaultContractResolver
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