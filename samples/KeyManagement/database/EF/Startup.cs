// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.KeyManagement.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace sample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public ILoggerFactory LoggerFactory { get; set; }

        public Startup(IConfiguration config, IHostingEnvironment environment, ILoggerFactory loggerFactory)
        {
            Configuration = config;
            Environment = environment;
            LoggerFactory = loggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cn = Configuration.GetConnectionString("db");

            services.AddDataProtection()
                .PersistKeysToDatabase(new DatabaseKeyManagementOptions
                {
                    ConfigureDbContext = b => b.UseSqlServer(cn),
                    LoggerFactory = LoggerFactory,
                });

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddSigningKeyManagement(
                    options => // configuring options is optional :)
                    {
                        // all of these values in here are changed for local testing
                        options.InitializationDuration = TimeSpan.FromSeconds(5);
                        options.InitializationSynchronizationDelay = TimeSpan.FromSeconds(1);

                        options.KeyActivationDelay = TimeSpan.FromSeconds(10);
                        options.KeyExpiration = TimeSpan.FromSeconds(20);
                        options.KeyRetirement = TimeSpan.FromSeconds(40);

                        // You can get your own license from:
                        // https://www.identityserver.com/products/KeyManagement
                        options.Licensee = "your licensee";
                        options.License = "your license key";
                    })
                    .PersistKeysToDatabase(new DatabaseKeyManagementOptions {
                        ConfigureDbContext = b => b.UseSqlServer(cn),
                    })
                    .ProtectKeysWithDataProtection()
                    //.EnableInMemoryCaching() // caching disabled unless explicitly enabled
                ;
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
        }
    }
}