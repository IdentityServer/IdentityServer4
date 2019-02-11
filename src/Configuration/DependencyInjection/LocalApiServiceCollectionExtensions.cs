// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// DI extension methods for adding IdentityServer
    /// </summary>
    public static class LocalApiServiceCollectionExtensions
    {
        /// <summary>
        /// Creates a builder.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddLocalApi(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<JwtBearerOptions>, IdentityServerJwtBearerOptionsConfiguration>());

            services.AddAuthentication()
                .AddJwtBearer("local", null, o => { });

            return services;
        }
    }

    internal class IdentityServerJwtBearerOptionsConfiguration : IConfigureNamedOptions<JwtBearerOptions>
    {
        public void Configure(string name, JwtBearerOptions options)
        {
            if (string.Equals(name, "local", StringComparison.Ordinal))
            {
                options.Events = options.Events ?? new JwtBearerEvents();
                options.Events.OnMessageReceived = ResolveAuthorityAndKeysAsync;
                options.Audience = "IdentityServerApi";

                var staticConfiguration = new OpenIdConnectConfiguration
                {
                    Issuer = options.Authority
                };

                var manager = new StaticConfigurationManager<OpenIdConnectConfiguration>(staticConfiguration);
                options.ConfigurationManager = manager;
                options.TokenValidationParameters.ValidIssuer = options.Authority;
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "role";
            }
        }

        internal static async Task ResolveAuthorityAndKeysAsync(AspNetCore.Authentication.JwtBearer.MessageReceivedContext messageReceivedContext)
        {
            var options = messageReceivedContext.Options;
            if (options.TokenValidationParameters.ValidIssuer == null || options.TokenValidationParameters.IssuerSigningKey == null)
            {
                var store = messageReceivedContext.HttpContext.RequestServices.GetRequiredService<ISigningCredentialStore>();
                var credential = await store.GetSigningCredentialsAsync();

                options.Authority = options.Authority ?? messageReceivedContext.HttpContext.GetIdentityServerIssuerUri();
                options.TokenValidationParameters.IssuerSigningKey = credential.Key;
                options.TokenValidationParameters.ValidIssuer = options.Authority;
            }
        }

        public void Configure(JwtBearerOptions options)
        {
        }
    }
}