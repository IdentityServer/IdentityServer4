using FluentAssertions;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.EndSession
{
    public class EndSessionTests
    {
        const string Category = "End session endpoint";

        MockIdSvrUiPipeline _mockPipeline = new MockIdSvrUiPipeline();

        public EndSessionTests()
        {
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client1/callback" },
                PostLogoutRedirectUris = new List<string> { "https://client1/signout-callback" },
                AllowAccessTokensViaBrowser = true
            });

            _mockPipeline.Users.Add(new InMemoryUser
            {
                Subject = "bob",
                Username = "bob",
                Claims = new Claim[]
                {
                    new Claim("name", "Bob Loblaw"),
                    new Claim("email", "bob@loblaw.com"),
                    new Claim("role", "Attorney"),
                }
            });

            _mockPipeline.Scopes.AddRange(new Scope[] {
                StandardScopes.OpenId
            });

            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _mockPipeline.Client.GetAsync(MockIdSvrUiPipeline.EndSessionEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_request_should_redirect_to_logout_page()
        {
            var response = await _mockPipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.EndSessionEndpoint);

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_redirect_to_configured_logout_path()
        {
            _mockPipeline.Options.UserInteractionOptions.LogoutUrl = "/logout";
            _mockPipeline.Options.UserInteractionOptions.LogoutIdParameter = "id";

            var response = await _mockPipeline.Client.GetAsync(MockIdSvrUiPipeline.EndSessionEndpoint);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://server/logout?id=");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task logout_request_with_params_should_pass_values_in_logout_context()
        {
            await _mockPipeline.LoginAsync(IdentityServerPrincipal.Create("bob", "Bob Loblaw"));

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token +
                "&post_logout_redirect_uri=https://client1/signout-callback");

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
            _mockPipeline.LogoutRequest.Should().NotBeNull();
            _mockPipeline.LogoutRequest.ClientId.Should().Be("client1");
            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().Be("https://client1/signout-callback");
            _mockPipeline.LogoutRequest.SignOutIFrameUrl.Should().StartWith(MockIdSvrUiPipeline.EndSessionCallbackEndpoint + "?sid=");
        }
    }
}
