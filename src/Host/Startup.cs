using Host.Configuration;
using IdentityServer4.Core.Services.InMemory;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.Core
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
            var cert = new X509Certificate2(_environment.ApplicationBasePath + "\\idsrv3test.pfx", "idsrv3test");

            services.AddIdentityServer(options =>
            {
                options.SigningCertificate = cert;
                options.IssuerUri = "https://idsrv4";
            });

            services.AddInMemoryClients(Clients.Get());
            services.AddInMemoryScopes(Scopes.Get());
            services.AddInMemoryUsers(Users.Get());
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Verbose);
            loggerFactory.AddDebug(LogLevel.Verbose);

            app.UseDeveloperExceptionPage();
            app.UseIISPlatformHandler();
            app.UseIdentityServer();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}