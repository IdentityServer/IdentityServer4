// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.Validation;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CryptoRandom = IdentityModel.CryptoRandom;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddExtensionGrantValidator<T>(this IIdentityServerBuilder builder)
            where T : class, IExtensionGrantValidator
        {
            builder.Services.AddTransient<IExtensionGrantValidator, T>();

            return builder;
        }

        public static IIdentityServerBuilder AddResourceOwnerValidator<T>(this IIdentityServerBuilder builder)
           where T : class, IResourceOwnerPasswordValidator
        {
            builder.Services.AddTransient<IResourceOwnerPasswordValidator, T>();

            return builder;
        }

        public static IIdentityServerBuilder AddSecretParser<T>(this IIdentityServerBuilder builder)
            where T : class, ISecretParser
        {
            builder.Services.AddTransient<ISecretParser, T>();

            return builder;
        }

        public static IIdentityServerBuilder AddSecretValidator<T>(this IIdentityServerBuilder builder)
            where T : class, ISecretValidator
        {
            builder.Services.AddTransient<ISecretValidator, T>();

            return builder;
        }

        /// <summary>
        /// Adds the client store cache.
        /// </summary>
        /// <typeparam name="T">The type of the concrete client store class that is registered in DI.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddClientStoreCache<T>(this IIdentityServerBuilder builder)
            where T : IClientStore
        {
            builder.Services.AddTransient<IClientStore, CachingClientStore<T>>();
            return builder;
        }
        
        /// <summary>
        /// Adds the client store cache.
        /// </summary>
        /// <typeparam name="T">The type of the concrete scope store class that is registered in DI.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddScopeStoreCache<T>(this IIdentityServerBuilder builder)
            where T : IScopeStore
        {
            builder.Services.AddTransient<IScopeStore, CachingScopeStore<T>>();
            return builder;
        }

        public static IIdentityServerBuilder SetSigningCredential(this IIdentityServerBuilder builder, SigningCredentials credential)
        {
            // todo
            //if (!(credential.Key is AsymmetricSecurityKey) &&
            //    !credential.Key.IsSupportedAlgorithm(SecurityAlgorithms.RsaSha256Signature))
            //{
            //    throw new InvalidOperationException("Signing key is not asymmetric and does not support RS256");
            //}

            builder.Services.AddSingleton<ISigningCredentialStore>(new InMemorySigningCredentialsStore(credential));
            builder.Services.AddSingleton<IValidationKeysStore>(new InMemoryValidationKeysStore(new[] { credential.Key }));

            return builder;
        }

        public static IIdentityServerBuilder SetSigningCredential(this IIdentityServerBuilder builder, X509Certificate2 certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("X509 certificate does not have a private key.");
            }

            var credential = new SigningCredentials(new X509SecurityKey(certificate), "RS256");
            return builder.SetSigningCredential(credential);
        }

        public static IIdentityServerBuilder SetSigningCredential(this IIdentityServerBuilder builder, string name, StoreLocation location = StoreLocation.LocalMachine)
        {
            X509Certificate2 certificate;

            if (location == StoreLocation.LocalMachine)
            {
                certificate = X509.LocalMachine.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
            }
            else
            {
                certificate = X509.CurrentUser.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
            }

            if (certificate == null) throw new InvalidOperationException($"certificate: '{name}' not found in certificate store");

            return builder.SetSigningCredential(certificate);
        }

        public static IIdentityServerBuilder SetSigningCredential(this IIdentityServerBuilder builder, RsaSecurityKey rsaKey)
        {
            if (!rsaKey.HasPrivateKey)
            {
                throw new InvalidOperationException("RSA key does not have a private key.");
            }

            var credential = new SigningCredentials(rsaKey, "RS256");
            return builder.SetSigningCredential(credential);
        }

        public static IIdentityServerBuilder SetTemporarySigningCredential(this IIdentityServerBuilder builder)
        {
            var rsa = RSA.Create();
            var key = new RsaSecurityKey(rsa)
            {
                KeyId = CryptoRandom.CreateUniqueId()
            };

            var credential = new SigningCredentials(key, "RS256");
            return builder.SetSigningCredential(credential);
        }

        public static IIdentityServerBuilder AddValidationKeys(this IIdentityServerBuilder builder, params AsymmetricSecurityKey[] keys)
        {
            builder.Services.AddSingleton<IValidationKeysStore>(new InMemoryValidationKeysStore(keys));

            return builder;
        }
    }
}