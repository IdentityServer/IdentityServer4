using Clients;
using IdentityModel;
using idunno.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace SampleApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
                .AddJsonFormatters()
                .AddAuthorization();

            services.AddCors();
            services.AddDistributedMemoryCache();

            services.AddAuthentication("token")
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = Constants.Authority;
                    options.RequireHttpsMetadata = false;

                    options.ApiName = "api1";
                    options.ApiSecret = "secret";
                })
                .AddCertificate("x509", options =>
                {
                    options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnValidateCertificate = context =>
                        {
                            context.Principal = Principal.CreateFromCertificate(context.ClientCertificate, includeAllClaims: true);
                            context.Success();

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "http://localhost:28895",
                    "http://localhost:7017");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseAuthentication();

            app.Use(async (ctx, next) =>
            {
                if (ctx.User.Identity.IsAuthenticated)
                {
                    var cnfJson = ctx.User.FindFirst("cnf")?.Value;
                    if (!String.IsNullOrWhiteSpace(cnfJson))
                    {
                        var certResult = await ctx.AuthenticateAsync("x509");
                        if (!certResult.Succeeded)
                        {
                            await ctx.ChallengeAsync("x509");
                            return;
                        }

                        var cert = ctx.Connection.ClientCertificate;
                        if (cert == null)
                        {
                            await ctx.ChallengeAsync("x509");
                            return;
                        }

                        var thumbprint = cert.Thumbprint;

                        var cnf = JObject.Parse(cnfJson);
                        var sha256 = cnf.Value<string>("x5t#S256");

                        if (String.IsNullOrWhiteSpace(sha256) ||
                            !thumbprint.Equals(sha256, StringComparison.OrdinalIgnoreCase))
                        {
                            await ctx.ChallengeAsync("token");
                            return;
                        }
                    }
                }

                await next();
            });

            app.UseMvc();
        }
    }
}