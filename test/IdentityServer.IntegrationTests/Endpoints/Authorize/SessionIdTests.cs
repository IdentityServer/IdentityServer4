﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using IdentityServer4.Models;
using System.Security.Claims;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Services.InMemory;

namespace IdentityServer4.IntegrationTests.Endpoints.Authorize
{
    public class SessionIdTests
    {
        const string Category = "SessionIdTests";

        MockIdSvrUiPipeline _mockPipeline = new MockIdSvrUiPipeline();

        public SessionIdTests()
        {
            _mockPipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client1",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid", "profile" },
                    RedirectUris = new List<string> { "https://client1/callback" },
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "client2",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = true,
                    AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                    RedirectUris = new List<string> { "https://client2/callback" },
                    AllowAccessTokensViaBrowser = true
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

            _mockPipeline.Initialize();
        }

        [Fact]
        public async Task session_id_should_be_reissued_if_session_cookie_absent()
        {
            await _mockPipeline.LoginAsync("bob");
            var sid1 = _mockPipeline.GetSessionCookie().Value;
            sid1.Should().NotBeNull();

            _mockPipeline.RemoveSessionCookie();

            await _mockPipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.DiscoveryEndpoint);

            var sid2 = _mockPipeline.GetSessionCookie().Value;
            sid2.Should().Be(sid1);
        }
    }
}
