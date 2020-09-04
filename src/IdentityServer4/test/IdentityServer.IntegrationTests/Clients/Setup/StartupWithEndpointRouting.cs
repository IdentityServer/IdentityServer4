using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer4.Configuration;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.IntegrationTests.Clients.Setup
{
    public class StartupWithEndpointRouting
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();

            services.AddRouting();

            var builder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = "https://idsvr4";

                options.Events = new EventsOptions
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true
                };
            });

            builder.AddInMemoryClients(Clients.Get());
            builder.AddInMemoryIdentityResources(Scopes.GetIdentityScopes());
            builder.AddInMemoryApiResources(Scopes.GetApiResources());
            builder.AddInMemoryApiScopes(Scopes.GetApiScopes());

            builder.AddDeveloperSigningCredential(persistKey: false);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapIdentityServer();
                });
        }
    }
}
