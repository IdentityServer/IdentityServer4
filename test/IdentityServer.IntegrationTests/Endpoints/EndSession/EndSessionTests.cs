using FluentAssertions;
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

        MockAuthorizationPipeline _mockPipeline = new MockAuthorizationPipeline();

        public EndSessionTests()
        {
            _mockPipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client1",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid", "profile" },
                    RedirectUris = new List<string> { "https://client1/callback" },
                    PostLogoutRedirectUris = new List<string> { "https://client1/signout-callback" },
                    AllowAccessTokensViaBrowser = true
                },
            });

            _mockPipeline.Users.Add(new InMemoryUser
            {
                Subject = "alice",
                Username = "alice",
                Claims = new Claim[]
                {
                    new Claim("name", "Alice Smith"),
                    new Claim("email", "alice@email.com"),
                    new Claim("role", "Manager"),
                }
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
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email
            });

            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _mockPipeline.Client.GetAsync(MockAuthorizationPipeline.EndSessionEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_redirect_to_configured_logout_path()
        {
            _mockPipeline.Options.UserInteractionOptions.LogoutUrl = "/logout";

            var response = await _mockPipeline.Client.GetAsync(MockAuthorizationPipeline.EndSessionEndpoint);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().Be("https://server/logout");
        }
    }
}
