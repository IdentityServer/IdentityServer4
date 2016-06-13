// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Validation;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder, List<InMemoryUser> users)
        {
            builder.Services.AddSingleton(users);

            builder.Services.AddTransient<IProfileService, InMemoryProfileService>();
            builder.Services.AddTransient<IResourceOwnerPasswordValidator, InMemoryResourceOwnerPasswordValidator>();
            
            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            builder.Services.AddSingleton(clients);

            builder.Services.AddTransient<IClientStore, InMemoryClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryScopes(this IIdentityServerBuilder builder, IEnumerable<Scope> scopes)
        {
            builder.Services.AddSingleton(scopes);
            builder.Services.AddTransient<IScopeStore, InMemoryScopeStore>();

            return builder;
        }

        public static IIdentityServerBuilder AddCustomGrantValidator<T>(this IIdentityServerBuilder builder)
            where T : class, ICustomGrantValidator
        {
            builder.Services.AddTransient<ICustomGrantValidator, T>();
            
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

        public static IIdentityServerBuilder SetSigningCredentials(this IIdentityServerBuilder builder, SigningCredentials credential)
        {
            builder.Services.AddSingleton<ISigningCredentialStore>(new InMemorySigningCredentialsStore(credential));
            builder.Services.AddSingleton<IValidationKeysStore>(new InMemoryValidationKeysStore(new[] { credential.Key }));

            return builder;
        }

        public static IIdentityServerBuilder SetSigningCredentials(this IIdentityServerBuilder builder, X509Certificate2 certificate)
        {
            // todo: need to set alg?
            var credential = new SigningCredentials(new X509SecurityKey(certificate), "RS256");

            return builder.SetSigningCredentials(credential);
        }
    }
}