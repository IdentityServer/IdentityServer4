﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Tests.Clients
{
    public class Startup
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
            builder.AddInMemoryUsers(Users.Get());
            builder.SetTemporarySigningCredential();

            builder.AddExtensionGrantValidator<ExtensionGrantValidator>();
            builder.AddExtensionGrantValidator<ExtensionGrantValidator2>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }
}