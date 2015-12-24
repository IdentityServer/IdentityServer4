using IdentityServer4.Core.Services.InMemory;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;

namespace IdentityServer4.Tests.Endpoints.Authorize
{
    public class Startup
    {
        private readonly IApplicationEnvironment _environment;

        public Startup(IApplicationEnvironment environment)
        {
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebEncoders();
            services.AddDataProtection();

            services.AddIdentityServer(options =>
            {
                options.SigningCertificate = _environment.LoadSigningCert();
            })
            .AddInMemoryClients(Clients.Get())
            .AddInMemoryScopes(Scopes.Get())
            .AddInMemoryUsers(new List<InMemoryUser>());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }
}