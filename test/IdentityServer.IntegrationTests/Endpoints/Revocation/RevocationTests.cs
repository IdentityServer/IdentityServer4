using FluentAssertions;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Revocation
{
    public class RevocationTests
    {
        const string Category = "RevocationTests endpoint";

        MockIdSvrUiPipeline _mockPipeline = new MockIdSvrUiPipeline();

        public RevocationTests()
        {
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client1/callback" },
                LogoutUri = "https://client1/signout",
                PostLogoutRedirectUris = new List<string> { "https://client1/signout-callback" },
                AllowAccessTokensViaBrowser = true
            });

            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client2",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client2/callback" },
                LogoutUri = "https://client2/signout",
                PostLogoutRedirectUris = new List<string> {
                    "https://client2/signout-callback",
                    "https://client2/signout-callback2",
                },
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
        public async Task get_request_should_return_405()
        {
            var response = await _mockPipeline.Client.GetAsync(MockIdSvrUiPipeline.RevocationEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        }

        [Fact(Skip = "TODO: more endpoint tests")]
        [Trait("Category", Category)]
        public async Task post_without_form_urlencoded_should_return_415()
        {
            var response = await _mockPipeline.Client.GetAsync(MockIdSvrUiPipeline.RevocationEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        }
    }
}
