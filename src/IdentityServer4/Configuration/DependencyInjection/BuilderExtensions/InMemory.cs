// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Services.Default;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerBuilderExtensionsInMemory
    {
        /// <summary>
        /// Adds the in memory caching.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryCaching(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.TryAddTransient(typeof(ICache<>), typeof(DefaultCache<>));

            return builder;
        }

        /// <summary>
        /// Adds the in memory scopes.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryScopes(this IIdentityServerBuilder builder, IEnumerable<Scope> scopes)
        {
            builder.Services.AddSingleton(scopes);
            builder.Services.AddTransient<IScopeStore, InMemoryScopeStore>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory clients.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="clients">The clients.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            builder.Services.AddSingleton(clients);

            builder.Services.AddTransient<IClientStore, InMemoryClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory users.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder, List<InMemoryUser> users)
        {
            builder.Services.AddSingleton(users);

            builder.Services.AddTransient<IProfileService, InMemoryUserProfileService>();
            builder.Services.AddTransient<IResourceOwnerPasswordValidator, InMemoryUserResourceOwnerPasswordValidator>();
            builder.Services.AddTransient<InMemoryUserLoginService>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory stores.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryStores(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IPersistedGrantStore, InMemoryPersistedGrantStore>();

            return builder;
        }
    }
}