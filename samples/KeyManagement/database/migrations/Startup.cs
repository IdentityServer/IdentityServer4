﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.KeyManagement.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace migrations
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cn = Configuration.GetConnectionString("db");

            services.AddKeyManagementDbContext(new DatabaseKeyManagementOptions {
                ConfigureDbContext = b =>
                     b.UseSqlServer(cn, dbOpts => dbOpts.MigrationsAssembly(typeof(Startup).Assembly.FullName))
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }
    }
}
