﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.ResponseHandling
{
    public class UserInfoResponseGeneratorTests
    {
        private UserInfoResponseGenerator _subject;
        private MockProfileService _mockProfileService = new MockProfileService();
        private ClaimsPrincipal _user;
        private Client _client;

        private InMemoryResourcesStore _resourceStore;
        private List<IdentityResource> _identityResources = new List<IdentityResource>();
        private List<ApiResource> _apiResources = new List<ApiResource>();

        public UserInfoResponseGeneratorTests()
        {
            _client = new Client
            {
                ClientId = "client"
            };

            _user = new IdentityServerUser("bob")
            {
                AdditionalClaims =
                {
                    new Claim("foo", "foo1"),
                    new Claim("foo", "foo2"),
                    new Claim("bar", "bar1"),
                    new Claim("bar", "bar2")
                }
            }.CreatePrincipal();

            _resourceStore = new InMemoryResourcesStore(_identityResources, _apiResources);
            _subject = new UserInfoResponseGenerator(_mockProfileService, _resourceStore, TestLogger.Create<UserInfoResponseGenerator>());
        }

        [Fact]
        public async Task GetRequestedClaimTypesAsync_when_no_scopes_requested_should_return_empty_claim_types()
        {
            var claims = await _subject.GetRequestedClaimTypesAsync(null);
            claims.Should().BeEquivalentTo(new string[] { });
        }

        [Fact]
        public async Task GetRequestedClaimTypesAsync_should_return_correct_identity_claims()
        {
            _identityResources.Add(new IdentityResource("id1", new[] { "c1", "c2" }));
            _identityResources.Add(new IdentityResource("id2", new[] { "c2", "c3" }));

            var claims = await _subject.GetRequestedClaimTypesAsync(new[] { "id1", "id2", "id3" });
            claims.Should().BeEquivalentTo(new string[] { "c1", "c2", "c3" });
        }

        [Fact]
        public async Task GetRequestedClaimTypesAsync_should_only_return_enabled_identity_claims()
        {
            _identityResources.Add(new IdentityResource("id1", new[] { "c1", "c2" }) { Enabled = false });
            _identityResources.Add(new IdentityResource("id2", new[] { "c2", "c3" }));

            var claims = await _subject.GetRequestedClaimTypesAsync(new[] { "id1", "id2", "id3" });
            claims.Should().BeEquivalentTo(new string[] { "c2", "c3" });
        }

        [Fact]
        public async Task ProcessAsync_should_call_profile_service_with_requested_claim_types()
        {
            _identityResources.Add(new IdentityResource("id1", new[] { "foo" }));
            _identityResources.Add(new IdentityResource("id2", new[] { "bar" }));

            var result = new UserInfoRequestValidationResult
            {
                Subject = _user,
                TokenValidationResult = new TokenValidationResult
                {
                    Claims = new List<Claim>
                    {
                        { new Claim("scope", "id1") },
                        { new Claim("scope", "id2") },
                        { new Claim("scope", "id3") }
                    },
                    Client = _client
                }
            };

            var claims = await _subject.ProcessAsync(result);

            _mockProfileService.GetProfileWasCalled.Should().BeTrue();
            _mockProfileService.ProfileContext.RequestedClaimTypes.Should().BeEquivalentTo(new[] { "foo", "bar" });
        }

        [Fact]
        public async Task ProcessAsync_should_return_claims_issued_by_profile_service()
        {
            _identityResources.Add(new IdentityResource("id1", new[] { "foo" }));
            _identityResources.Add(new IdentityResource("id2", new[] { "bar" }));
            _mockProfileService.ProfileClaims = new[]
            {
                new Claim("email", "fred@gmail.com"),
                new Claim("name", "fred jones")
            };

            var result = new UserInfoRequestValidationResult
            {
                Subject = _user,
                TokenValidationResult = new TokenValidationResult
                {
                    Claims = new List<Claim>
                    {
                        { new Claim("scope", "id1") },
                        { new Claim("scope", "id2") },
                        { new Claim("scope", "id3") }
                    },
                    Client = _client
                }
            };

            var claims = await _subject.ProcessAsync(result);

            claims.Should().ContainKey("email");
            claims["email"].Should().Be("fred@gmail.com");
            claims.Should().ContainKey("name");
            claims["name"].Should().Be("fred jones");
        }

        [Fact]
        public async Task ProcessAsync_should_return_sub_from_user()
        {
            _identityResources.Add(new IdentityResource("id1", new[] { "foo" }));
            _identityResources.Add(new IdentityResource("id2", new[] { "bar" }));

            var result = new UserInfoRequestValidationResult
            {
                Subject = _user,
                TokenValidationResult = new TokenValidationResult
                {
                    Claims = new List<Claim>
                    {
                        { new Claim("scope", "id1") },
                        { new Claim("scope", "id2") },
                        { new Claim("scope", "id3") }
                    },
                    Client = _client
                }
            };

            var claims = await _subject.ProcessAsync(result);

            claims.Should().ContainKey("sub");
            claims["sub"].Should().Be("bob");
        }

        [Fact]
        public void ProcessAsync_should_throw_if_incorrect_sub_issued_by_profile_service()
        {
            _identityResources.Add(new IdentityResource("id1", new[] { "foo" }));
            _identityResources.Add(new IdentityResource("id2", new[] { "bar" }));
            _mockProfileService.ProfileClaims = new[]
            {
                new Claim("sub", "fred")
            };

            var result = new UserInfoRequestValidationResult
            {
                Subject = _user,
                TokenValidationResult = new TokenValidationResult
                {
                    Claims = new List<Claim>
                    {
                        { new Claim("scope", "id1") },
                        { new Claim("scope", "id2") },
                        { new Claim("scope", "id3") }
                    },
                    Client = _client
                }
            };

            Func<Task> act = () => _subject.ProcessAsync(result);

            act.Should().Throw<InvalidOperationException>()
                .And.Message.Should().Contain("subject");
        }

    }
}