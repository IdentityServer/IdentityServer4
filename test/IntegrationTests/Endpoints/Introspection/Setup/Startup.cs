using IdentityServer4.Core.Services.InMemory;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.Tests.Endpoints.Introspection
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
            var cert = new X509Certificate2(Path.Combine(_environment.ApplicationBasePath, "idsrv3test.pfx"), "idsrv3test");

            var builder = services.AddIdentityServer(options =>
            {
                options.SigningCertificate = cert;
                options.IssuerUri = "https://idsrv4";
                options.Endpoints.EnableAuthorizeEndpoint = false;
            });

            builder.AddInMemoryClients(Clients.Get());
            builder.AddInMemoryScopes(Scopes.Get());
            builder.AddInMemoryUsers(new List<InMemoryUser>());
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseIdentityServer();
        }
    }
}