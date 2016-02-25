using FluentAssertions;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Core;
using Xunit;

namespace IdentityServer4.Tests.Endpoints.EndSession
{
    public class EndSessionTests
    {
        const string Category = "End session endpoint";

        private const string invalidIdToken = "eyJ0eX";

        MockAuthorizationPipeline _mockPipeline = new MockAuthorizationPipeline();

        public EndSessionTests()
        {
            _mockPipeline.Clients.AddRange(Clients.Get());

            _mockPipeline.Users.Add(new InMemoryUser
            {
                Subject = "bob",
                Username = "bob",
                Claims = new[]
                   {
                        new Claim("name", "Bob Loblaw"),
                        new Claim("email", "bob@loblaw.com"),
                        new Claim("role", "Attorney"),
                   }
            });

            _mockPipeline.Scopes.AddRange(Scopes.Get());
            
            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Request()
        {
            var form = new Dictionary<string, string>();

            var response = await _mockPipeline.Client.PostAsync(IdentityServerPipeline.EndSessionEndpoint, new FormUrlEncodedContent(form));

            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("Invalid request");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_Token()
        {
            var requestUri = $"{IdentityServerPipeline.EndSessionEndpoint}/?post_logout_redirect_uri=http://client/callback.html";

            var response = await _mockPipeline.Client.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();
            
            content.Should().Contain("Id token hint not found");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Token()
        {
            var requestUri = $"{IdentityServerPipeline.EndSessionEndpoint}?id_token_hint={invalidIdToken}";

            var response = await _mockPipeline.Client.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();
            
            content.Should().Contain("Invalid id token hint");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_redirect_uri_to_client()
        {
            _mockPipeline.BrowserClient.AllowAutoRedirect = false;

            await _mockPipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            
            var url = _mockPipeline.CreateAuthorizeUrl(
                          clientId: "client1",
                          responseType: "id_token",
                          scope: "openid",
                          redirectUri: "http://client/index.html",
                          state: state,
                          nonce: nonce);

            var authenticationResponse = await _mockPipeline.BrowserClient.GetAsync(url);

            var authorizationResponse = _mockPipeline.ParseAuthorizationResponseUrl(authenticationResponse.Headers.Location.ToString());

            var requestUri = $"{IdentityServerPipeline.EndSessionEndpoint}?id_token_hint={authorizationResponse.IdentityToken}&post_logout_redirect_uri=http://client/invalid_callback.html";

            var response = await _mockPipeline.Client.GetAsync(requestUri);

            var content = await response.Content.ReadAsStringAsync();
            
            content.Should().Contain("Invalid post logout URI for this client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Should_redirect_to_logout_url()
        {
            _mockPipeline.BrowserClient.AllowAutoRedirect = false;

            await _mockPipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();

            var url = _mockPipeline.CreateAuthorizeUrl(
                          clientId: "client1",
                          responseType: "id_token",
                          scope: "openid",
                          redirectUri: "http://client/index.html",
                          state: state,
                          nonce: nonce);

            var authenticationResponse = await _mockPipeline.BrowserClient.GetAsync(url);

            var authorizationResponse = _mockPipeline.ParseAuthorizationResponseUrl(authenticationResponse.Headers.Location.ToString());

            var requestUri = $"{IdentityServerPipeline.EndSessionEndpoint}?id_token_hint={authorizationResponse.IdentityToken}&state={state}&post_logout_redirect_uri=http://client/callback.html";

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;

            var response = await _mockPipeline.Client.GetAsync(requestUri);

            response.Headers.Location.AbsoluteUri.Should().Contain(Constants.RoutePaths.Logout);
        }
    }
}
