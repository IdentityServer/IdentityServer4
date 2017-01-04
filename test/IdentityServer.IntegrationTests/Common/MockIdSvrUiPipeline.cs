// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using IdentityServer4.Services;
using System.Security.Claims;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using FluentAssertions;
using System.Net;
using IdentityServer4.Configuration;

namespace IdentityServer4.IntegrationTests.Common
{
    public class MockIdSvrUiPipeline : IdentityServerPipeline
    {
        public const string FederatedSignOutPath = "/signout-oidc";
        public const string FederatedSignOutUrl = "https://server" + FederatedSignOutPath;

        public RequestDelegate Login { get; set; }
        public RequestDelegate Logout { get; set; }
        public RequestDelegate Consent { get; set; }
        public RequestDelegate Error { get; set; }
        public RequestDelegate FederatedSignOut { get; set; }

        public MockIdSvrUiPipeline()
        {
            Login = OnLogin;
            Logout = OnLogout;
            Consent = OnConsent;
            Error = OnError;
            FederatedSignOut = OnFederatedSignOut;

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
                var idSvrOptions = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();
                idSvrOptions.Authentication.AuthenticationScheme = CookieAuthenticationScheme;
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
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

            app.Map(FederatedSignOutPath, path =>
            {
                path.Run(ctx => FederatedSignOut(ctx));
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
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            LoginRequest = await interaction.GetAuthorizationContextAsync(ctx.Request.Query["returnUrl"].FirstOrDefault());
        }

        async Task IssueLoginCookie(HttpContext ctx)
        {
            if (CookieAuthenticationScheme != null && Subject != null)
            {
                await ctx.Authentication.SignInAsync(CookieAuthenticationScheme, Subject);
                Subject = null;
                var url = ctx.Request.Query[this.Options.UserInteraction.LoginReturnUrlParameter].FirstOrDefault();
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
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            LogoutRequest = await interaction.GetLogoutContextAsync(ctx.Request.Query["logoutId"].FirstOrDefault());
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
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            ConsentRequest = await interaction.GetAuthorizationContextAsync(ctx.Request.Query["returnUrl"].FirstOrDefault());
        }

        async Task CreateConsentResponse(HttpContext ctx)
        {
            if (ConsentRequest != null && ConsentResponse != null)
            {
                var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
                await interaction.GrantConsentAsync(ConsentRequest, ConsentResponse);
                ConsentResponse = null;

                var url = ctx.Request.Query[this.Options.UserInteraction.ConsentReturnUrlParameter].FirstOrDefault();
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
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            ErrorMessage = await interaction.GetErrorContextAsync(ctx.Request.Query["errorId"].FirstOrDefault());
        }

        Task OnFederatedSignOut(HttpContext ctx)
        {
            return Task.FromResult(0);
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
            var user = Users.Single(x => x.SubjectId == subject);
            var name = user.Claims.Where(x => x.Type == "name").Select(x => x.Value).FirstOrDefault() ?? user.Username;
            await LoginAsync(IdentityServerPrincipal.Create(subject, name));
        }

        public void RemoveLoginCookie()
        {
            BrowserClient.RemoveCookie("https://server/", ".AspNetCore.cookie_authn");
        }
        public void RemoveSessionCookie()
        {
            BrowserClient.RemoveCookie("https://server/", $"{Options.Authentication.EffectiveAuthenticationScheme}.session");
        }
        public Cookie GetSessionCookie()
        {
            return BrowserClient.GetCookie("https://server/", $"{Options.Authentication.EffectiveAuthenticationScheme}.session");
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
            string codeChallenge = null,
            string codeChallengeMethod = null,
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
                codeChallenge: codeChallenge,
                codeChallengeMethod: codeChallengeMethod,
                extra: extra);
            return url;
        }

        public IdentityModel.Client.AuthorizeResponse ParseAuthorizationResponseUrl(string url)
        {
            return new IdentityModel.Client.AuthorizeResponse(url);
        }

        public async Task<IdentityModel.Client.AuthorizeResponse> RequestAuthorizationEndpointAsync(
            string clientId,
            string responseType,
            string scope = null,
            string redirectUri = null,
            string state = null,
            string nonce = null,
            string loginHint = null,
            string acrValues = null,
            string responseMode = null,
            string codeChallenge = null,
            string codeChallengeMethod = null,
            object extra = null)
        {
            var old = BrowserClient.AllowAutoRedirect;
            BrowserClient.AllowAutoRedirect = false;

            var url = CreateAuthorizeUrl(clientId, responseType, scope, redirectUri, state, nonce, loginHint, acrValues, responseMode, codeChallenge, codeChallengeMethod, extra);
            var result = await BrowserClient.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            BrowserClient.AllowAutoRedirect = old;

            var redirect = result.Headers.Location.ToString();
            if (redirect.StartsWith(IdentityServerPipeline.ErrorPage))
            {
                // request error page in pipeline so we can get error info
                await BrowserClient.GetAsync(redirect);
                
                // no redirect to client
                return null;
            }

            return new IdentityModel.Client.AuthorizeResponse(redirect);
        }
    }
}