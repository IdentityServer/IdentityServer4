// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder, List<InMemoryUser> users)
        {
            builder.Services.AddInstance(users);
            builder.Services.AddTransient<IUserService, InMemoryUserService>();

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            var clientStore = new InMemoryClientStore(clients);
            builder.Services.AddSingleton<IClientStore>(prov => clientStore);

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryScopes(this IIdentityServerBuilder builder, IEnumerable<Scope> scopes)
        {
            var scopeStore = new InMemoryScopeStore(scopes);
            builder.Services.AddSingleton<IScopeStore>(prov => scopeStore);

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
    }
}