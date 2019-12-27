// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Authorize
{
    public class RestrictAccessTokenViaBrowserTests
    {
        private const string Category = "RestrictAccessTokenViaBrowserTests";

        private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();

        private ClaimsPrincipal _user = new IdentityServerUser("bob").CreatePrincipal();

        public RestrictAccessTokenViaBrowserTests()
        {
            _mockPipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client1",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid" },
                    RedirectUris = new List<string> { "https://client1/callback" },
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "client2",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid" },
                    RedirectUris = new List<string> { "https://client2/callback" },
                    AllowAccessTokensViaBrowser = false
                },
                new Client
                {
                    ClientId = "client3",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid" },
                    RedirectUris = new List<string> { "https://client3/callback" },
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "client4",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid" },
                    RedirectUris = new List<string> { "https://client4/callback" },
                    AllowAccessTokensViaBrowser = false
                }
            });

            _mockPipeline.Users.Add(new TestUser
            {
                SubjectId = "bob",
                Username = "bob",
                Claims = new Claim[]
                {
                    new Claim("name", "Bob Loblaw"),
                    new Claim("email", "bob@loblaw.com"),
                    new Claim("role", "Attorney")
                }
            });

            _mockPipeline.IdentityScopes.AddRange(new IdentityResource[] {
                new IdentityResources.OpenId()
            });

            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unrestricted_implicit_client_can_request_IdToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client1",
                "id_token", "openid", "https://client1/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.AbsoluteUri.Should().StartWith("https://client1/callback");
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IdentityToken.Should().NotBeNull();
            authorization.AccessToken.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unrestricted_implicit_client_can_request_IdTokenToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client1",
                "id_token token", "openid", "https://client1/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.AbsoluteUri.Should().StartWith("https://client1/callback");
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IdentityToken.Should().NotBeNull();
            authorization.AccessToken.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_implicit_client_can_request_IdToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client2",
                "id_token", "openid", "https://client2/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.AbsoluteUri.Should().StartWith("https://client2/callback");
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IdentityToken.Should().NotBeNull();
            authorization.AccessToken.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_implicit_client_cannot_request_IdTokenToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client2",
                "id_token token", "openid", "https://client2/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            _mockPipeline.ErrorWasCalled.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unrestricted_hybrid_client_can_request_CodeIdToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client3",
                "code id_token", "openid", "https://client3/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.AbsoluteUri.Should().StartWith("https://client3/callback");
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IdentityToken.Should().NotBeNull();
            authorization.AccessToken.Should().BeNull();
            authorization.Code.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unrestricted_hybrid_client_can_request_CodeIdTokenToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client3",
                "code id_token token", "openid", "https://client3/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.AbsoluteUri.Should().StartWith("https://client3/callback");
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IdentityToken.Should().NotBeNull();
            authorization.AccessToken.Should().NotBeNull();
            authorization.Code.Should().NotBeNull();
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_hybrid_client_can_request_CodeIdToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client4",
                "code id_token", "openid", "https://client4/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.AbsoluteUri.Should().StartWith("https://client4/callback");
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IdentityToken.Should().NotBeNull();
            authorization.AccessToken.Should().BeNull();
            authorization.Code.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_hybrid_client_cannot_request_CodeIdTokenToken()
        {
            await _mockPipeline.LoginAsync(_user);

            var url = _mockPipeline.CreateAuthorizeUrl("client4",
                "code id_token token", "openid", "https://client4/callback", "state", "nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            _mockPipeline.ErrorWasCalled.Should().BeTrue();
        }
    }
}
