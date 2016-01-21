// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
using IdentityModel.Client;

namespace IdentityServer4.Tests.Common
{
    public class MockAuthorizationPipeline : IdentityServerPipeline
    {
        public RequestDelegate Login { get; set; }
        public RequestDelegate Consent { get; set; }
        public RequestDelegate Error { get; set; }

        public MockAuthorizationPipeline()
        {
            Login = OnLogin;
            Consent = OnConsent;
            Error = OnError;

            this.OnConfigureServices += MockAuthorizationPipeline_OnConfigureServices;
            this.OnPreConfigure += MockAuthorizationPipeline_OnPreConfigure;
            this.OnPostConfigure += MockAuthorizationPipeline_OnPostConfigure;
        }

        private void MockAuthorizationPipeline_OnConfigureServices(IServiceCollection obj)
        {
        }

        private void MockAuthorizationPipeline_OnPreConfigure(IApplicationBuilder app)
        {
            if (CookieAuthenticationScheme != null)
            {
                this.Options.AuthenticationOptions.PrimaryAuthenticationScheme = CookieAuthenticationScheme;
                app.UseCookieAuthentication(options =>
                {
                    options.AuthenticationScheme = CookieAuthenticationScheme;
                });
            }
        }

        private void MockAuthorizationPipeline_OnPostConfigure(IApplicationBuilder app)
        {
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
                SignInRequest = await interaction.GetRequestAsync(ctx.Request.Query["id"].First());
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
                await interaction.ProcessResponseAsync(ctx.Request.Query["id"].First(), SignInResponse);
            }
        }

        public bool ConsentWasCalled { get; set; }
        public ConsentRequest ConsentRequest { get; set; }
        public ConsentResponse ConsentResponse { get; set; }

        async Task OnConsent(HttpContext ctx)
        {
            ConsentWasCalled = true;
            await ReadConsentMessage(ctx);
            await CreateConsentResponse(ctx);
        }

        async Task ReadConsentMessage(HttpContext ctx)
        {
            try
            {
                var interaction = ctx.RequestServices.GetRequiredService<ConsentInteraction>();
                ConsentRequest = await interaction.GetRequestAsync(ctx.Request.Query["id"].First());
            }
            catch { }
        }

        async Task CreateConsentResponse(HttpContext ctx)
        {
            if (ConsentResponse != null)
            {
                var interaction = ctx.RequestServices.GetRequiredService<ConsentInteraction>();
                await interaction.ProcessResponseAsync(ctx.Request.Query["id"].First(), ConsentResponse);
                ConsentResponse = null;
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
                ErrorMessage = await interaction.GetRequestAsync(ctx.Request.Query["id"].First());
            }
            catch { }
        }

        /* helpers */
        public async Task LoginAsync(ClaimsPrincipal subject)
        {
            var old = Browser.AllowAutoRedirect;
            Browser.AllowAutoRedirect = false;

            Subject = subject;
            await BrowserClient.GetAsync(LoginPage);

            Browser.AllowAutoRedirect = old;
        }

        public async Task LoginAsync(string subject)
        {
            var user = Users.Single(x => x.Subject == subject);
            var name = user.Claims.Where(x => x.Type == "name").Select(x => x.Value).FirstOrDefault() ?? user.Username;
            await LoginAsync(IdentityServerPrincipal.Create(subject, name));
        }

        public string CreateAuthorizeUrl(
            string clientId,
            string responseType,
            string scope = null,
            string redirectUri = null,
            string state = null,
            string nonce = null,
            string loginHint = null,
            string acrValues = null,
            string responseMode = null,
            object extra = null)
        {
            var url = new AuthorizeRequest(AuthorizeEndpoint).CreateAuthorizeUrl(
                clientId: clientId,
                responseType: responseType,
                scope: scope,
                redirectUri: redirectUri,
                state: state,
                nonce: nonce,
                loginHint: loginHint,
                acrValues: acrValues,
                responseMode: responseMode,
                extra: extra);
            return url;
        }

        public IdentityModel.Client.AuthorizeResponse ParseAuthorizationResponseUrl(string url)
        {
            return new IdentityModel.Client.AuthorizeResponse(url);
        }
    }
}