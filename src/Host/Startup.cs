// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
using Microsoft.AspNetCore.Authentication.OAuth;

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

            // only use for development until this bug is fixed
            // https://github.com/aspnet/DependencyInjection/pull/470
            return services.BuildServiceProvider(validateScopes: true);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            //Google简单开发框架
            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                ClientId = "826677679029-31bi1q2u69k9ounnorgqi6j7mlp0rk57.apps.googleusercontent.com",
                ClientSecret = "s31uY-vFN3e-5dijl0qv195R"
            });

            app.UseGitHubAuthentication(options =>
            {
                options.AuthenticationScheme = "Github";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "6e8f81dfbf8d03070d7f";
                options.ClientSecret = "a01140a56b5a16b820b57ec53b66ad2dcd1d3977";
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
                        
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}