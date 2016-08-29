// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class QuickStartIdentityServerServiceCollectionExtensions
    {
        public static IIdentityServerBuilder AddIdentityServerQuickstart(this IServiceCollection services)
        {
            var builder = services.AddIdentityServer();
            builder.AddInMemoryStores();
            builder.SetTemporarySigningCredential();

            return builder;
        }

        public static IIdentityServerBuilder AddIdentityServerQuickstart(this IServiceCollection services, Action<IdentityServerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityServerQuickstart();
        }

        public static IIdentityServerBuilder AddIdentityServerQuickstart(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentityServerOptions>(configuration);
            return services.AddIdentityServerQuickstart();
        }
    }
}