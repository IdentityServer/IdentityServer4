// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IdentityServer4.IntegrationTests.Common
{
    public class IdentityServerPipeline
    {
        public const string LoginPage = "https://server/account/login";
        public const string ConsentPage = "https://server/account/consent";
        public const string ErrorPage = "https://server/home/error";

        public const string DiscoveryEndpoint = "https://server/.well-known/openid-configuration";
        public const string DiscoveryKeysEndpoint = "https://server/.well-known/openid-configuration/jwks";
        public const string AuthorizeEndpoint = "https://server/connect/authorize";
        public const string TokenEndpoint = "https://server/connect/token";
        public const string RevocationEndpoint = "https://server/connect/revocation";
        public const string UserInfoEndpoint = "https://server/connect/userinfo";
        public const string IntrospectionEndpoint = "https://server/connect/introspect";
        public const string IdentityTokenValidationEndpoint = "https://server/connect/identityTokenValidation";
        public const string EndSessionEndpoint = "https://server/connect/endsession";
        public const string EndSessionCallbackEndpoint = "https://server/connect/endsession/callback";
        public const string CheckSessionEndpoint = "https://server/connect/checksession";

        public IdentityServerOptions Options { get; set; }
        public List<Client> Clients { get; set; } = new List<Client>();
        public List<IdentityResource> IdentityScopes { get; set; } = new List<IdentityResource>();
        public List<ApiResource> ApiScopes { get; set; } = new List<ApiResource>();
        public List<TestUser> Users { get; set; } = new List<TestUser>();

        public TestServer Server { get; set; }
        public HttpMessageHandler Handler { get; set; }

        public BrowserClient BrowserClient { get; set; }
        public HttpClient Client { get; set; }

        public void Initialize(string basePath = null)
        {
            var builder = new WebHostBuilder();
            builder.ConfigureServices(ConfigureServices);
            builder.Configure(app=>
            {
                if (basePath != null)
                {
                    app.Map(basePath, map =>
                    {
                        ConfigureApp(map);
                    });
                }
                else
                {
                    ConfigureApp(app);
                }
            });

            Server = new TestServer(builder);
            Handler = Server.CreateHandler();

            BrowserClient = new BrowserClient(new BrowserHandler(Handler));
            Client = new HttpClient(Handler);
        }

        public event Action<IServiceCollection> OnConfigureServices = x => { };

        public void ConfigureServices(IServiceCollection services)
        {
            OnConfigureServices(services);

            services.AddDataProtection();

            services.AddIdentityServer(options =>
            {
                Options = options;

                options.Events = new EventsOptions
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true
                };
            })
            .AddInMemoryClients(Clients)
            .AddInMemoryIdentityResources(IdentityScopes)
            .AddInMemoryApiResources(ApiScopes)
            .AddTestUsers(Users)
            .AddDeveloperSigningCredential(persistKey: false);
        }

        public event Action<IApplicationBuilder> OnPreConfigure = x => { };
        public event Action<IApplicationBuilder> OnPostConfigure = x => { };
         
        public void ConfigureApp(IApplicationBuilder app)
        {
            OnPreConfigure(app);
            app.UseIdentityServer();
            OnPostConfigure(app);
        }
    }
}
