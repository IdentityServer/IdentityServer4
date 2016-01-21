// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using IdentityModel.Client;
using IdentityServer4.Tests.Common;
using IdentityServer4.Core;
using System.Collections.Generic;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using System.Security.Claims;
using System.Net.Http;

namespace IdentityServer4.Tests.Cors
{
    public class CorsTests
    {
        const string Category = "CORS Integration";

        MockAuthorizationPipeline _pipeline = new MockAuthorizationPipeline();

        public CorsTests()
        {
            _pipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client",
                    Flow = Flows.Implicit,
                    RequireConsent = true,
                    AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                    RedirectUris = new List<string> { "https://client/callback" },
                    AllowedCorsOrigins = new List<string> { "https://client" }
                }
            });

            _pipeline.Users.Add(new InMemoryUser
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

            _pipeline.Scopes.AddRange(new Scope[] {
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
                }
            });

            _pipeline.Initialize();
        }

        [Theory]
        [InlineData(MockAuthorizationPipeline.DiscoveryEndpoint)]
        [InlineData(MockAuthorizationPipeline.DiscoveryKeysEndpoint)]
        [InlineData(MockAuthorizationPipeline.TokenEndpoint)]
        [InlineData(MockAuthorizationPipeline.UserInfoEndpoint)]
        [InlineData(MockAuthorizationPipeline.IdentityTokenValidationEndpoint)]
        [InlineData(MockAuthorizationPipeline.RevocationEndpoint)]
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
        [InlineData(MockAuthorizationPipeline.AuthorizeEndpoint)]
        [InlineData(MockAuthorizationPipeline.EndSessionEndpoint)]
        [InlineData(MockAuthorizationPipeline.CheckSessionEndpoint)]
        [InlineData(MockAuthorizationPipeline.LoginPage)]
        [InlineData(MockAuthorizationPipeline.ConsentPage)]
        [InlineData(MockAuthorizationPipeline.ErrorPage)]
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
