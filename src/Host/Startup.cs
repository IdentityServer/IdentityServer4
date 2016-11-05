// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Host.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Serilog.Events;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using IdentityServer4;
using IdentityServer4.Validation;
using Serilog;
using Microsoft.AspNetCore.Http;

namespace Host
{
    public class Startup
    {
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

            services.AddIdentityServer(options =>
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
                .AddInMemoryUsers(Users.Get())
                .AddTemporarySigningCredential()
                .AddExtensionGrantValidator<Extensions.ExtensionGrantValidator>()
                .AddSecretParser<ClientAssertionSecretParser>()
                .AddSecretValidator<PrivateKeyJwtSecretValidator>();

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
        
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Filter.ByIncludingOnly(serilogFilter)
                .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                .WriteTo.File(@"identityserver4_log.txt")
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
                AuthenticationScheme = "oidc",
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

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "aad",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                SignOutScheme = IdentityServerConstants.SignoutScheme,
                DisplayName = "AAD",
                Authority = "https://login.windows.net/4ca9cb4c-5e5f-4be9-b700-c532992a3705",
                ClientId = "96e3c53e-01cb-4244-b658-a42164cb67a9",
                ResponseType = "id_token",
                Scope = { "openid profile" },
                CallbackPath = new PathString("/signin-aad"),
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
