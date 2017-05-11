﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Host.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4;
using IdentityServer4.Validation;
using Serilog;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Hosting;

namespace Host
{
    public class Startup
    {
        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment environment)
        {
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(@"identityserver4_log.txt");
                
            if (environment.IsDevelopment())
            {
                serilog.WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}");
            }

            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    { "IdentityServer4", LogLevel.Debug },
                    { "Microsoft", LogLevel.Information },
                    { "System", LogLevel.Error }
                })
                .AddSerilog(serilog.CreateLogger());
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer(options =>
                {
                    options.Authentication.FederatedSignOutPaths.Add("/signout-callback-aad");
                    options.Authentication.FederatedSignOutPaths.Add("/signout-callback-idsrv");
                    options.Authentication.FederatedSignOutPaths.Add("/signout-callback-adfs");

                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                })
                .AddInMemoryClients(Clients.Get())
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddDeveloperSigningCredential()
                .AddExtensionGrantValidator<Extensions.ExtensionGrantValidator>()
                .AddExtensionGrantValidator<Extensions.NoSubjectExtensionGrantValidator>()
                .AddSecretParser<ClientAssertionSecretParser>()
                .AddSecretValidator<PrivateKeyJwtSecretValidator>()
                .AddRedirectUriValidator<StrictRedirectUriValidatorAppAuth>()
                .AddTestUsers(TestUsers.Users);

            services.AddMvc();

            return services.BuildServiceProvider(validateScopes: true);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com",
                ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "demoidsrv",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                SignOutScheme = IdentityServerConstants.SignoutScheme,
                AutomaticChallenge = false,
                DisplayName = "IdentityServer",
                Authority = "https://demo.identityserver.io/",
                ClientId = "implicit",
                ResponseType = "id_token",
                Scope = { "openid profile" },
                SaveTokens = true,
                CallbackPath = new PathString("/signin-idsrv"),
                SignedOutCallbackPath = new PathString("/signout-callback-idsrv"),
                RemoteSignOutPath = new PathString("/signout-idsrv"),
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
                AutomaticChallenge = false,
                DisplayName = "AAD",
                Authority = "https://login.windows.net/4ca9cb4c-5e5f-4be9-b700-c532992a3705",
                ClientId = "96e3c53e-01cb-4244-b658-a42164cb67a9",
                ResponseType = "id_token",
                Scope = { "openid profile" },
                CallbackPath = new PathString("/signin-aad"),
                SignedOutCallbackPath = new PathString("/signout-callback-aad"),
                RemoteSignOutPath = new PathString("/signout-aad"),
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                }
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "adfs",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                SignOutScheme = IdentityServerConstants.SignoutScheme,
                AutomaticChallenge = false,
                DisplayName = "ADFS",
                Authority = "https://adfs.leastprivilege.vm/adfs",
                ClientId = "c0ea8d99-f1e7-43b0-a100-7dee3f2e5c3c",
                ResponseType = "id_token",
                Scope = { "openid profile" },
                CallbackPath = new PathString("/signin-adfs"),
                SignedOutCallbackPath = new PathString("/signout-callback-adfs"),
                RemoteSignOutPath = new PathString("/signout-adfs"),
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
