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

            builder.Services.AddTransient<IProfileService, InMemoryProfileService>();
            builder.Services.AddTransient<IResourceOwnerPasswordValidator, InMemoryResourceOwnerPasswordValidator>();
            
            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            builder.Services.AddInstance(clients);
            builder.Services.AddTransient<IClientStore, InMemoryClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryScopes(this IIdentityServerBuilder builder, IEnumerable<Scope> scopes)
        {
            builder.Services.AddInstance(scopes);
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
    }
}