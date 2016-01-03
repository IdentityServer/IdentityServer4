using IdentityServer4.Core;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using IdentityServer4.Core.Services;
using System.Security.Claims;
using System.Diagnostics;

namespace IdentityServer4.Tests.Common
{
    public class MockAuthorizationPipeline
    {
        private readonly IApplicationEnvironment _environment;
        private IEnumerable<Client> _clients;
        private IEnumerable<Scope> _scopes;
        private List<InMemoryUser> _users;

        public RequestDelegate Login { get; set; }
        public RequestDelegate Consent { get; set; }
        public RequestDelegate Error { get; set; }

        public MockAuthorizationPipeline(IApplicationEnvironment environment)
        {
            _environment = environment;
            Login = OnLogin;
            Consent = OnConsent;
            Error = OnError;
        }

        public MockAuthorizationPipeline(IEnumerable<Client> clients, IEnumerable<Scope> scopes, List<InMemoryUser> users)
            : this(PlatformServices.Default.Application)
        {
           _clients = clients;
           _scopes = scopes;
           _users = users;
        }

        public string CookieAuthenticationScheme { get; set; } = "cookie_authn";

        public bool LoginWasCalled { get; set; }
        public SignInRequest SignInRequest { get; set; }
        public ClaimsPrincipal Subject { get; set; }
        public SignInResponse SignInResponse { get; set; }

        async Task OnLogin(HttpContext ctx)
        {
            LoginWasCalled = true;
            await ReadSignInMessage(ctx);
            await IssueLoginCookie(ctx);
            await CreateSignInResponse(ctx);
        }

        async Task ReadSignInMessage(HttpContext ctx)
        {
            try
            {
                var interaction = ctx.RequestServices.GetRequiredService<SignInInteraction>();
                SignInRequest = await interaction.GetRequestAsync();
            }
            catch(Exception ex) {
                    var msg = ex.ToString();
                Trace.Write(msg);
            }
        }

        async Task IssueLoginCookie(HttpContext ctx)
        {
            if (CookieAuthenticationScheme != null && Subject != null)
            {
                await ctx.Authentication.SignInAsync(CookieAuthenticationScheme, Subject);
                Subject = null;
            }
        }

        async Task CreateSignInResponse(HttpContext ctx)
        {
            if (SignInResponse != null)
            {
                var interaction = ctx.RequestServices.GetRequiredService<SignInInteraction>();
                await interaction.ProcessResponseAsync(SignInResponse);
            }
        }

        public bool ConsentWasCalled { get; set; }
        public ConsentRequest ConsentRequest { get; set; }
        public ConsentResponse ConsentResponse { get; set; }

        async Task OnConsent(HttpContext ctx)
        {
            ConsentWasCalled = true;
            await ReadConsentMessage(ctx);
        }

        async Task ReadConsentMessage(HttpContext ctx)
        {
            try
            {
                var interaction = ctx.RequestServices.GetRequiredService<ConsentInteraction>();
                ConsentRequest = await interaction.GetRequestAsync();
            }
            catch { }
        }

        async Task CreateConsentResponse(HttpContext ctx)
        {
            if (ConsentResponse != null)
            {
                var interaction = ctx.RequestServices.GetRequiredService<ConsentInteraction>();
                await interaction.ProcessResponseAsync(ConsentResponse);
            }
        }

        public bool ErrorWasCalled { get; set; }
        public ErrorMessage ErrorMessage { get; set; }

        async Task OnError(HttpContext ctx)
        {
            ErrorWasCalled = true;
            await ReadErrorMessage(ctx);
        }

        async Task ReadErrorMessage(HttpContext ctx)
        {
            try
            {
                var interaction = ctx.RequestServices.GetRequiredService<ErrorInteraction>();
                ErrorMessage = await interaction.GetRequestAsync();
            }
            catch { }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebEncoders();
            services.AddDataProtection();

            services.AddIdentityServer(options =>
            {
                options.SigningCertificate = _environment.LoadSigningCert();
                options.AuthenticationOptions.PrimaryAuthenticationScheme = CookieAuthenticationScheme;
            })
            .AddInMemoryClients(_clients)
            .AddInMemoryScopes(_scopes)
            .AddInMemoryUsers(_users);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(async (ctx, next) =>
            {
                await next();
            });

            if (CookieAuthenticationScheme != null)
            {
                app.UseCookieAuthentication(options =>
                {
                    options.AuthenticationScheme = CookieAuthenticationScheme;
                });
            }

            app.Use(async (ctx, next) =>
            {
                await next();
            });

            app.UseIdentityServer();

            app.Use(async (ctx, next) =>
            {
                await next();
            });

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