using Microsoft.AspNet.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Threading;
using System;
using IdentityServer4.Core.Extensions;
using IdentityModel.Client;
using IdentityServer4.Tests.Common;
using IdentityServer4.Core;
using System.Collections.Generic;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using System.Security.Claims;

namespace IdentityServer4.Tests.Endpoints.Authorize
{
    public class AuthorizeTests
    {
        const string Category = "Authorize endpoint";
        const string LoginEndpoint = "https://server/ui/login";
        const string AuthorizeEndpoint = "https://server/connect/authorize";

        private readonly HttpClient _client;
        private readonly Browser _browser;
        private readonly HttpMessageHandler _handler;
        private readonly MockAuthorizationPipeline _mockPipeline;
        private readonly AuthorizeRequest _authorizeRequest = new AuthorizeRequest(AuthorizeEndpoint);

        public AuthorizeTests()
        {
            _mockPipeline = new MockAuthorizationPipeline(GetClients(), GetScopes(), GetUsers());
            var server = TestServer.Create(null, _mockPipeline.Configure, _mockPipeline.ConfigureServices);
            _handler = server.CreateHandler();
            _browser = new Browser(_handler);
            _client = new HttpClient(_browser);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _client.GetAsync(AuthorizeEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_500()
        {
            var response = await _client.GetAsync(AuthorizeEndpoint);

            ((int)response.StatusCode).Should().BeLessThan(500);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task anonymous_user_requesting_authorization_should_be_redirected_to_login_page_with_signin_request()
        {
            var url = _authorizeRequest.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _client.GetAsync(url);

            _mockPipeline.LoginWasCalled.Should().BeTrue();
            _mockPipeline.SignInRequest.Should().NotBeNull();
            _mockPipeline.SignInRequest.ClientId.Should().Be("client1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authenticated_user_with_valid_request_should_recieve_authorization_response()
        {
            _browser.AllowAutoRedirect = false;

            _mockPipeline.Subject = IdentityServerPrincipal.Create("123", "Bob Loblaw");
            await _client.GetAsync(LoginEndpoint);

            var url = _authorizeRequest.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://client1/callback");

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeFalse();
            authorization.IdentityToken.Should().NotBeNull();
            authorization.State.Should().Be("123_state");
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client1",
                    Flow = Flows.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid", "profile" },
                    RedirectUris = new List<string> { "https://client1/callback" }
                },
            };
        }
        public static IEnumerable<Scope> GetScopes()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                new Scope
                {
                    Name = "api1",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "api2",
                    Type = ScopeType.Resource
                },
            };
        }
        public static List<InMemoryUser> GetUsers()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = "123",
                    Username = "bob",
                    Claims = new Claim[]
                    {
                        new Claim("name", "Bob Loblaw"),
                        new Claim("email", "bob@loblaw.com"),
                        new Claim("role", "Attorney"),
                    }
                }
            };
        }
    }
}
