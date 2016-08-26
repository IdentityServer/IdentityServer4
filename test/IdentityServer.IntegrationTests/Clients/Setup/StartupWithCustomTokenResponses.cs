// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Tests.Clients
{
    public class StartupWithCustomTokenResponses
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();

            var builder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = "https://idsvr4";
            });

            builder.AddInMemoryClients(Clients.Get());
            builder.AddInMemoryScopes(Scopes.Get());
            builder.SetTemporarySigningCredential();

            services.AddTransient<IResourceOwnerPasswordValidator, CustomResponseResourceOwnerValidator>();
            builder.AddExtensionGrantValidator<CustomResponseExtensionGrantValidator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }
}