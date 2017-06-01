// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Collections.Generic;
using IdentityServer4.Models;
using System.Security.Claims;
using System.Net.Http;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Test;

namespace IdentityServer4.IntegrationTests.Pipeline
{
    public class CorsTests
    {
        const string Category = "CORS Integration";

        MockIdSvrUiPipeline _pipeline = new MockIdSvrUiPipeline();

        public CorsTests()
        {
            _pipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = true,
                    AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                    RedirectUris = new List<string> { "https://client/callback" },
                    AllowedCorsOrigins = new List<string> { "https://client" }
                }
            });

            _pipeline.Users.Add(new TestUser
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

            _pipeline.IdentityScopes.AddRange(new IdentityResource[] {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            });
            _pipeline.ApiScopes.AddRange(new ApiResource[] {
                new ApiResource
                {
                    Name = "api",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "api2"
                        }
                    }
                }
            });

            _pipeline.Initialize();
        }

        [Theory]
        [InlineData(MockIdSvrUiPipeline.DiscoveryEndpoint)]
        [InlineData(MockIdSvrUiPipeline.DiscoveryKeysEndpoint)]
        [InlineData(MockIdSvrUiPipeline.TokenEndpoint)]
        [InlineData(MockIdSvrUiPipeline.UserInfoEndpoint)]
        [InlineData(MockIdSvrUiPipeline.RevocationEndpoint)]
        [Trait("Category", Category)]
        public async Task cors_request_to_allowed_endpoints_should_succeed(string url)
        {
            _pipeline.Client.DefaultRequestHeaders.Add("Origin", "https://client");
            _pipeline.Client.DefaultRequestHeaders.Add("Access-Control-Request-Method", "GET");
            
            var message = new HttpRequestMessage(HttpMethod.Options, url);
            var response = await _pipeline.Client.SendAsync(message);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        }

        [Theory]
        [InlineData(MockIdSvrUiPipeline.AuthorizeEndpoint)]
        [InlineData(MockIdSvrUiPipeline.EndSessionEndpoint)]
        [InlineData(MockIdSvrUiPipeline.CheckSessionEndpoint)]
        [InlineData(MockIdSvrUiPipeline.LoginPage)]
        [InlineData(MockIdSvrUiPipeline.ConsentPage)]
        [InlineData(MockIdSvrUiPipeline.ErrorPage)]
        [Trait("Category", Category)]
        public async Task cors_request_to_restricted_endpoints_should_not_succeed(string url)
        {
            _pipeline.Client.DefaultRequestHeaders.Add("Origin", "https://client");
            _pipeline.Client.DefaultRequestHeaders.Add("Access-Control-Request-Method", "GET");

            var message = new HttpRequestMessage(HttpMethod.Options, url);
            var response = await _pipeline.Client.SendAsync(message);

            response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
        }
    }
}
