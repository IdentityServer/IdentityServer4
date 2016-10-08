// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Host.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Serilog.Events;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using IdentityServer4;
using IdentityServer4.Validation;
using Serilog;

namespace Host
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IISOptions>(options=>
            {
                const string windowsAuthType = "Negotiate";
                var windows = options.AuthenticationDescriptions
                    .FirstOrDefault(x => x.AuthenticationScheme == windowsAuthType);
                if (windows != null)
                {
                    windows.DisplayName = "Windows";
                }
            });

            var builder = services.AddDeveloperIdentityServer(options =>
            {
                //options.EventsOptions = new EventsOptions
                //{
                //    RaiseErrorEvents = true,
                //    RaiseFailureEvents = true,
                //    RaiseInformationEvents = true,
                //    RaiseSuccessEvents = true
                //};

                options.AuthenticationOptions.FederatedSignOutPaths.Add("/signout-oidc");
            })
            .AddInMemoryClients(Clients.Get())
            .AddInMemoryScopes(Scopes.Get())
            .AddInMemoryUsers(Users.Get());
            
            builder.AddExtensionGrantValidator<Extensions.ExtensionGrantValidator>();

            builder.AddSecretParser<ClientAssertionSecretParser>();
            builder.AddSecretValidator<PrivateKeyJwtSecretValidator>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // serilog filter
            Func<LogEvent, bool> serilogFilter = (e) =>
            {
                var context = e.Properties["SourceContext"].ToString();

                return (context.StartsWith("\"IdentityServer") ||
                        context.StartsWith("\"IdentityModel") ||
                        e.Level == LogEventLevel.Error ||
                        e.Level == LogEventLevel.Fatal);
            };
        
            // built-in logging filter
            Func<string, LogLevel, bool> filter = (scope, level) =>
                scope.StartsWith("IdentityServer") ||
                scope.StartsWith("IdentityModel") ||
                level == LogLevel.Error ||
                level == LogLevel.Critical;

            //loggerFactory.AddConsole(filter);
            //loggerFactory.AddDebug(filter);

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Filter.ByIncludingOnly(serilogFilter)
                .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                .WriteTo.File(@"c:\logs\IdentityServer4.txt")
                .CreateLogger();

            loggerFactory.AddSerilog(serilog);

            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });

            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                ClientId = "998042782978-s07498t8i8jas7npj4crve1skpromf37.apps.googleusercontent.com",
                ClientSecret = "HsnwJri_53zn7VcO1Fm7THBb",
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                SignOutScheme = IdentityServerConstants.SignoutScheme,
                DisplayName = "IdentityServer3",
                Authority = "https://demo.identityserver.io/",
                ClientId = "implicit",
                ResponseType = "id_token",
                Scope = { "openid profile" },
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                }
            });

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
