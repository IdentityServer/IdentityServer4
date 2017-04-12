// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// DI extension methods for adding IdentityServer
    /// </summary>
    public static class IdentityServerServiceCollectionExtensions
    {
        /// <summary>
        /// Creates a builder.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServerBuilder(this IServiceCollection services)
        {
            return new IdentityServerBuilder(services);
        }

        /// <summary>
        /// Adds IdentityServer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services)
        {
            var builder = services.AddIdentityServerBuilder();

            builder.AddRequiredPlatformServices();

            builder.AddCoreServices();
            builder.AddDefaultEndpoints();
            builder.AddPluggableServices();
            builder.AddValidators();
            builder.AddResponseGenerators();

            builder.AddDefaultSecretParsers();
            builder.AddDefaultSecretValidators();

            // provide default in-memory implementation, not suitable for most production scenarios
            builder.AddInMemoryPersistedGrants();

            return new IdentityServerBuilder(services);
        }

        /// <summary>
        /// Adds IdentityServer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityServer();
        }

        /// <summary>
        /// Adds the IdentityServer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentityServerOptions>(configuration);
            return services.AddIdentityServer();
        }
    }
}