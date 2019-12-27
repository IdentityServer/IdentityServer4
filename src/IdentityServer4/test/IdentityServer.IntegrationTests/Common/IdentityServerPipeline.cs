// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer.IntegrationTests.Common
{
    public class IdentityServerPipeline
    {
        public const string BaseUrl = "https://server";
        public const string LoginPage = BaseUrl + "/account/login";
        public const string ConsentPage = BaseUrl + "/account/consent";
        public const string ErrorPage = BaseUrl + "/home/error";

        public const string DeviceAuthorization = BaseUrl + "/connect/deviceauthorization";
        public const string DiscoveryEndpoint = BaseUrl + "/.well-known/openid-configuration";
        public const string DiscoveryKeysEndpoint = BaseUrl + "/.well-known/openid-configuration/jwks";
        public const string AuthorizeEndpoint = BaseUrl + "/connect/authorize";
        public const string TokenEndpoint = BaseUrl + "/connect/token";
        public const string RevocationEndpoint = BaseUrl + "/connect/revocation";
        public const string UserInfoEndpoint = BaseUrl + "/connect/userinfo";
        public const string IntrospectionEndpoint = BaseUrl + "/connect/introspect";
        public const string IdentityTokenValidationEndpoint = BaseUrl + "/connect/identityTokenValidation";
        public const string EndSessionEndpoint = BaseUrl + "/connect/endsession";
        public const string EndSessionCallbackEndpoint = BaseUrl + "/connect/endsession/callback";
        public const string CheckSessionEndpoint = BaseUrl + "/connect/checksession";

        public const string FederatedSignOutPath = "/signout-oidc";
        public const string FederatedSignOutUrl = BaseUrl + FederatedSignOutPath;

        public IdentityServerOptions Options { get; set; }
        public List<Client> Clients { get; set; } = new List<Client>();
        public List<IdentityResource> IdentityScopes { get; set; } = new List<IdentityResource>();
        public List<ApiResource> ApiScopes { get; set; } = new List<ApiResource>();
        public List<TestUser> Users { get; set; } = new List<TestUser>();

        public TestServer Server { get; set; }
        public HttpMessageHandler Handler { get; set; }

        public BrowserClient BrowserClient { get; set; }
        public HttpClient BackChannelClient { get; set; }

        public MockMessageHandler BackChannelMessageHandler { get; set; } = new MockMessageHandler();
        public MockMessageHandler JwtRequestMessageHandler { get; set; } = new MockMessageHandler();

        public event Action<IServiceCollection> OnPreConfigureServices = services => { };
        public event Action<IServiceCollection> OnPostConfigureServices = services => { };
        public event Action<IApplicationBuilder> OnPreConfigure = app => { };
        public event Action<IApplicationBuilder> OnPostConfigure = app => { };

        public Func<HttpContext, Task<bool>> OnFederatedSignout;

        public void Initialize(string basePath = null, bool enableLogging = false)
        {
            var builder = new WebHostBuilder();
            builder.ConfigureServices(ConfigureServices);
            builder.Configure(app=>
            {
                if (basePath != null)
                {
                    app.Map(basePath, map =>
                    {
                        ConfigureApp(map);
                    });
                }
                else
                {
                    ConfigureApp(app);
                }
            });

            if (enableLogging)
            {
                builder.ConfigureLogging((ctx, b) => b.AddConsole());
            }

            Server = new TestServer(builder);
            Handler = Server.CreateHandler();
            
            BrowserClient = new BrowserClient(new BrowserHandler(Handler));
            BackChannelClient = new HttpClient(Handler);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            OnPreConfigureServices(services);

            services.AddAuthentication(opts =>
            {
                opts.AddScheme("external", scheme =>
                {
                    scheme.DisplayName = "External";
                    scheme.HandlerType = typeof(MockExternalAuthenticationHandler);
                });
            });
            services.AddTransient<MockExternalAuthenticationHandler>(svcs =>
            {
                var handler = new MockExternalAuthenticationHandler(svcs.GetRequiredService<IHttpContextAccessor>());
                if (OnFederatedSignout != null) handler.OnFederatedSignout = OnFederatedSignout;
                return handler;
            });

            services.AddIdentityServer(options =>
            {
                Options = options;

                options.Events = new EventsOptions
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true
                };
            })
            .AddInMemoryClients(Clients)
            .AddInMemoryIdentityResources(IdentityScopes)
            .AddInMemoryApiResources(ApiScopes)
            .AddTestUsers(Users)
            .AddDeveloperSigningCredential(persistKey: false);

            services.AddHttpClient()
                .AddHttpClient<BackChannelLogoutHttpClient>()
                .AddHttpMessageHandler(() => BackChannelMessageHandler);

            services.AddHttpClient<JwtRequestUriHttpClient>()
                .AddHttpMessageHandler(() => JwtRequestMessageHandler);

            OnPostConfigureServices(services);
        }

        public void ConfigureApp(IApplicationBuilder app)
        {
            OnPreConfigure(app);

            app.UseIdentityServer();

            // UI endpoints
            app.Map(Constants.UIConstants.DefaultRoutePaths.Login.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => OnLogin(ctx));
            });
            app.Map(Constants.UIConstants.DefaultRoutePaths.Logout.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => OnLogout(ctx));
            });
            app.Map(Constants.UIConstants.DefaultRoutePaths.Consent.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => OnConsent(ctx));
            });
            app.Map(Constants.UIConstants.DefaultRoutePaths.Error.EnsureLeadingSlash(), path =>
            {
                path.Run(ctx => OnError(ctx));
            });

            OnPostConfigure(app);
        }

        public bool LoginWasCalled { get; set; }
        public AuthorizationRequest LoginRequest { get; set; }
        public ClaimsPrincipal Subject { get; set; }
        public bool FollowLoginReturnUrl { get; set; }

        private async Task OnLogin(HttpContext ctx)
        {
            LoginWasCalled = true;
            await ReadLoginRequest(ctx);
            await IssueLoginCookie(ctx);
        }

        private async Task ReadLoginRequest(HttpContext ctx)
        {
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            LoginRequest = await interaction.GetAuthorizationContextAsync(ctx.Request.Query["returnUrl"].FirstOrDefault());
        }

        private async Task IssueLoginCookie(HttpContext ctx)
        {
            if (Subject != null)
            {
                var props = new AuthenticationProperties();
                await ctx.SignInAsync(Subject, props);
                Subject = null;
                var url = ctx.Request.Query[Options.UserInteraction.LoginReturnUrlParameter].FirstOrDefault();
                if (url != null)
                {
                    ctx.Response.Redirect(url);
                }
            }
        }

        public bool LogoutWasCalled { get; set; }
        public LogoutRequest LogoutRequest { get; set; }

        private async Task OnLogout(HttpContext ctx)
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

        private async Task OnConsent(HttpContext ctx)
        {
            ConsentWasCalled = true;
            await ReadConsentMessage(ctx);
            await CreateConsentResponse(ctx);
        }

        private async Task ReadConsentMessage(HttpContext ctx)
        {
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            ConsentRequest = await interaction.GetAuthorizationContextAsync(ctx.Request.Query["returnUrl"].FirstOrDefault());
        }

        private async Task CreateConsentResponse(HttpContext ctx)
        {
            if (ConsentRequest != null && ConsentResponse != null)
            {
                var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
                await interaction.GrantConsentAsync(ConsentRequest, ConsentResponse);
                ConsentResponse = null;

                var url = ctx.Request.Query[Options.UserInteraction.ConsentReturnUrlParameter].FirstOrDefault();
                if (url != null)
                {
                    ctx.Response.Redirect(url);
                }
            }
        }

        public bool ErrorWasCalled { get; set; }
        public ErrorMessage ErrorMessage { get; set; }

        private async Task OnError(HttpContext ctx)
        {
            ErrorWasCalled = true;
            await ReadErrorMessage(ctx);
        }

        private async Task ReadErrorMessage(HttpContext ctx)
        {
            var interaction = ctx.RequestServices.GetRequiredService<IIdentityServerInteractionService>();
            ErrorMessage = await interaction.GetErrorContextAsync(ctx.Request.Query["errorId"].FirstOrDefault());
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
            await LoginAsync(new IdentityServerUser(subject).CreatePrincipal());
        }

        public void RemoveLoginCookie()
        {
            BrowserClient.RemoveCookie(BaseUrl, IdentityServerConstants.DefaultCookieAuthenticationScheme);
        }
        public void RemoveSessionCookie()
        {
            BrowserClient.RemoveCookie(BaseUrl, IdentityServerConstants.DefaultCheckSessionCookieName);
        }
        public Cookie GetSessionCookie()
        {
            return BrowserClient.GetCookie(BaseUrl, IdentityServerConstants.DefaultCheckSessionCookieName);
        }

        public string CreateAuthorizeUrl(
            string clientId = null,
            string responseType = null,
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
            var url = new RequestUrl(AuthorizeEndpoint).CreateAuthorizeUrl(
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

        public AuthorizeResponse ParseAuthorizationResponseUrl(string url)
        {
            return new AuthorizeResponse(url);
        }

        public async Task<AuthorizeResponse> RequestAuthorizationEndpointAsync(
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

            return new AuthorizeResponse(redirect);
        }
    }

    public class MockMessageHandler : DelegatingHandler
    {
        public bool InvokeWasCalled { get; set; }
        public Func<HttpRequestMessage, Task> OnInvoke { get; set; }
        public HttpResponseMessage Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InvokeWasCalled = true;

            if (OnInvoke != null)
            {
                await OnInvoke.Invoke(request);
            }
            return Response;
        }
    }

    public class MockExternalAuthenticationHandler : 
        IAuthenticationHandler,
        IAuthenticationSignInHandler,
        IAuthenticationRequestHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public Func<HttpContext, Task<bool>> OnFederatedSignout = 
            async context =>
            {
                await context.SignOutAsync();
                return true;
            };

        public MockExternalAuthenticationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HandleRequestAsync()
        {
            if (HttpContext.Request.Path == IdentityServerPipeline.FederatedSignOutPath)
            {
                return await OnFederatedSignout(HttpContext);
            }

            return false;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            return Task.CompletedTask;
        }

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        public Task SignOutAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }
    }
}
