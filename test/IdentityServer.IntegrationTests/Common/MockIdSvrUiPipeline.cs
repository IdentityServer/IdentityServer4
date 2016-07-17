// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Linq;
using IdentityServer4.Services;
using System.Security.Claims;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using IdentityServer4.Validation;

namespace IdentityServer4.Tests.Common
{
    public class MockIdSvrUiPipeline : IdentityServerPipeline
    {
        public RequestDelegate Login { get; set; }
        public RequestDelegate Logout { get; set; }
        public RequestDelegate Consent { get; set; }
        public RequestDelegate Error { get; set; }

        public MockIdSvrUiPipeline()
        {
            Login = OnLogin;
            Logout = OnLogout;
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
                this.Options.AuthenticationOptions.AuthenticationScheme = CookieAuthenticationScheme;
                app.UseCookieAuthentication(new CookieAuthenticationOptions {
                    AuthenticationScheme = CookieAuthenticationScheme
                });
            }
        }

        private void MockAuthorizationPipeline_OnPostConfigure(IApplicationBuilder app)
        {
            app.Map(Constants.UIConstants.DefaultRoutePaths.Login.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Login(ctx));
            });

            app.Map(Constants.UIConstants.DefaultRoutePaths.Logout.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Logout(ctx));
            });

            app.Map(Constants.UIConstants.DefaultRoutePaths.Consent.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Consent(ctx));
            });

            app.Map(Constants.UIConstants.DefaultRoutePaths.Error.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => Error(ctx));
            });
        }

        public string CookieAuthenticationScheme { get; set; } = "cookie_authn";

        public bool LoginWasCalled { get; set; }
        public AuthorizationRequest LoginRequest { get; set; }
        public ClaimsPrincipal Subject { get; set; }
        public bool FollowLoginReturnUrl { get; set; }

        async Task OnLogin(HttpContext ctx)
        {
            LoginWasCalled = true;
            await ReadLoginRequest(ctx);
            await IssueLoginCookie(ctx);
        }

        async Task ReadLoginRequest(HttpContext ctx)
        {
            var interaction = ctx.RequestServices.GetRequiredService<IUserInteractionService>();
            LoginRequest = await interaction.GetLoginContextAsync();
        }

        async Task IssueLoginCookie(HttpContext ctx)
        {
            if (CookieAuthenticationScheme != null && Subject != null)
            {
                await ctx.Authentication.SignInAsync(CookieAuthenticationScheme, Subject);
                Subject = null;
                var url = ctx.Request.Query[this.Options.UserInteractionOptions.LoginReturnUrlParameter].FirstOrDefault();
                if (url != null)
                {
                    ctx.Response.Redirect(url);
                }
            }
        }

        public bool LogoutWasCalled { get; set; }
        public LogoutRequest LogoutRequest { get; set; }

        async Task OnLogout(HttpContext ctx)
        {
            LogoutWasCalled = true;
            await ReadLogoutRequest(ctx);
        }

        private async Task ReadLogoutRequest(HttpContext ctx)
        {
            var interaction = ctx.RequestServices.GetRequiredService<IUserInteractionService>();
            LogoutRequest = await interaction.GetLogoutContextAsync();
        }

        public bool ConsentWasCalled { get; set; }
        public AuthorizationRequest ConsentRequest { get; set; }
        public ConsentResponse ConsentResponse { get; set; }

        async Task OnConsent(HttpContext ctx)
        {
            ConsentWasCalled = true;
            await ReadConsentMessage(ctx);
            await CreateConsentResponse(ctx);
        }

        async Task ReadConsentMessage(HttpContext ctx)
        {
            var interaction = ctx.RequestServices.GetRequiredService<IUserInteractionService>();
            ConsentRequest = await interaction.GetConsentContextAsync();
        }

        async Task CreateConsentResponse(HttpContext ctx)
        {
            if (ConsentRequest != null && ConsentResponse != null)
            {
                var interaction = ctx.RequestServices.GetRequiredService<IUserInteractionService>();
                await interaction.GrantConsentAsync(ConsentRequest, ConsentResponse);
                ConsentResponse = null;

                var url = ctx.Request.Query[this.Options.UserInteractionOptions.ConsentReturnUrlParameter].FirstOrDefault();
                if (url != null)
                {
                    ctx.Response.Redirect(url);
                }
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
            var interaction = ctx.RequestServices.GetRequiredService<IUserInteractionService>();
            ErrorMessage = await interaction.GetErrorContextAsync();
        }

        /* helpers */
        public async Task LoginAsync(ClaimsPrincipal subject)
        {
            var old = BrowserClient.AllowAutoRedirect;
            BrowserClient.AllowAutoRedirect = false;

            Subject = subject;
            await BrowserClient.GetAsync(LoginPage);

            BrowserClient.AllowAutoRedirect = old;
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