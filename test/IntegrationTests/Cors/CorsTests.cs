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

        IdentityServerPipeline _pipeline = new IdentityServerPipeline();

        public CorsTests()
        {
            _pipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client",
                    Flow = Flows.Implicit,
                    RequireConsent = true,
                    AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                    RedirectUris = new List<string> { "https://client/callback" }
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

        [Theory(Skip = "todo: not done")]
        [InlineData("http://server/connect/authorize")]
        [Trait("Category", Category)]
        public async Task cors_request_to_allowed_endpoints_should_succeed(string url)
        {
            _pipeline.Client.DefaultRequestHeaders.Add("Origin", "http://client");

            var response = await _pipeline.Client.GetAsync(IdentityServerPipeline.AuthorizeEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }
    }
}
