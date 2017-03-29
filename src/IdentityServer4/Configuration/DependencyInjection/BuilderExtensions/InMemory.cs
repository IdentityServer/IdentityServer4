// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering in-memory services
    /// </summary>
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
        /// Adds the in memory identity resources.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="identityResources">The identity resources.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryIdentityResources(this IIdentityServerBuilder builder, IEnumerable<IdentityResource> identityResources)
        {
            builder.Services.AddSingleton(identityResources);
            builder.AddResourceStore<InMemoryResourcesStore>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory API resources.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="apiResources">The API resources.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryApiResources(this IIdentityServerBuilder builder, IEnumerable<ApiResource> apiResources)
        {
            builder.Services.AddSingleton(apiResources);
            builder.AddResourceStore<InMemoryResourcesStore>();

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

            builder.AddClientStore<InMemoryClientStore>();
            builder.AddCorsPolicyService<InMemoryCorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory stores.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryPersistedGrants(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IPersistedGrantStore, InMemoryPersistedGrantStore>();

            return builder;
        }
    }
}