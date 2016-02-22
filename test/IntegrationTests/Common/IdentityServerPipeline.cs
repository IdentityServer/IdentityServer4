// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Tests.Common;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Common
{
    public class IdentityServerPipeline
    {
        public const string LoginPage = "https://server/ui/login";
        public const string ConsentPage = "https://server/ui/consent";
        public const string ErrorPage = "https://server/ui/error";

        public const string DiscoveryEndpoint = "https://server/.well-known/openid-configuration";
        public const string DiscoveryKeysEndpoint = "https://server/.well-known/openid-configuration/jwks";
        public const string AuthorizeEndpoint = "https://server/connect/authorize";
        public const string TokenEndpoint = "https://server/connect/token";
        public const string RevocationEndpoint = "https://server/connect/revocation";
        public const string UserInfoEndpoint = "https://server/connect/userinfo";
        public const string IntrospectionEndpoint = "https://server/connect/introspect";
        public const string IdentityTokenValidationEndpoint = "https://server/connect/identityTokenValidation";
        public const string EndSessionEndpoint = "https://server/connect/endsession";
        public const string CheckSessionEndpoint = "https://server/connect/checksession";

        public IdentityServerOptions Options { get; set; } = new IdentityServerOptions();
        public List<Client> Clients { get; set; } = new List<Client>();
        public List<Scope> Scopes { get; set; } = new List<Scope>();
        public List<InMemoryUser> Users { get; set; } = new List<InMemoryUser>();

        public TestServer Server { get; set; }
        public HttpMessageHandler Handler { get; set; }

        public BrowserClient BrowserClient { get; set; }
        public HttpClient Client { get; set; }

        public IdentityServerPipeline(IApplicationEnvironment environment)
        {
            Options.SigningCertificate = environment.LoadSigningCert();
        }

        public IdentityServerPipeline()
            : this(PlatformServices.Default.Application)
        {
        }

        public void Initialize()
        {
            Server = TestServer.Create(null, Configure, ConfigureServices);
            Handler = Server.CreateHandler();

            BrowserClient = new BrowserClient(new BrowserHandler(Handler));
            Client = new HttpClient(Handler);
        }

        public event Action<IServiceCollection> OnConfigureServices = x => { };

        public void ConfigureServices(IServiceCollection services)
        {
            OnConfigureServices(services);

            services.AddDataProtection();

            services.AddIdentityServer(Options)
                .AddInMemoryClients(Clients)
                .AddInMemoryScopes(Scopes)
                .AddInMemoryUsers(Users);
        }

        public event Action<IApplicationBuilder> OnPreConfigure = x => { };
        public event Action<IApplicationBuilder> OnPostConfigure = x => { };
         
        public void Configure(IApplicationBuilder app)
        {
            OnPreConfigure(app);
            app.UseIdentityServer();
            OnPostConfigure(app);
        }
    }
}
