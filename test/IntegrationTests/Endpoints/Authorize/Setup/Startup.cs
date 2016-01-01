using IdentityServer4.Core;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Services.InMemory;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Endpoints.Authorize
{
    public class Startup
    {
        public RequestDelegate Login { get; set; } = ctx => Task.FromResult(0);
        public RequestDelegate Consent { get; set; } = ctx => Task.FromResult(0);
        public RequestDelegate Error { get; set; } = ctx => Task.FromResult(0);

        private readonly IApplicationEnvironment _environment;

        public Startup()
            : this(PlatformServices.Default.Application)
        {
        }

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
                options.AuthenticationOptions.PrimaryAuthenticationScheme = "cookie_auth";
            })
            .AddInMemoryClients(Clients.Get())
            .AddInMemoryScopes(Scopes.Get())
            .AddInMemoryUsers(new List<InMemoryUser>());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = "cookie_auth";
            });

            app.UseIdentityServer();

            app.Map(Constants.RoutePaths.Login.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Login(ctx));
            });

            app.Map(Constants.RoutePaths.Consent.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Consent(ctx));
            });

            app.Map(Constants.RoutePaths.Error.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Error(ctx));
            });
        }
    }
}