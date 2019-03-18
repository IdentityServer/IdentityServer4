﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Test;

namespace IdentityServer4.IntegrationTests.Conformance.Basic
{
    public class RedirectUriTests
    {
        private const string Category = "Conformance.Basic.RedirectUriTests";

        private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();

        public RedirectUriTests()
        {
            _mockPipeline.Initialize();

            _mockPipeline.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = "code_client",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha512())
                },

                AllowedGrantTypes = GrantTypes.Code,
                AllowedScopes = { "openid" },

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    "https://code_client/callback",
                    "https://code_client/callback?foo=bar&baz=quux"
                }
            });

            _mockPipeline.IdentityScopes.Add(new IdentityResources.OpenId());

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
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Reject_redirect_uri_not_matching_registered_redirect_uri()
        {
            await _mockPipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();

            var url = _mockPipeline.CreateAuthorizeUrl(
                           clientId: "code_client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://bad",
                           state: state,
                           nonce: nonce);
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorWasCalled.Should().BeTrue();
            _mockPipeline.ErrorMessage.Error.Should().Be("unauthorized_client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Reject_request_without_redirect_uri_when_multiple_registered()
        {
            await _mockPipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();

            var url = _mockPipeline.CreateAuthorizeUrl(
                          clientId: "code_client",
                          responseType: "code",
                          scope: "openid",
                          // redirectUri deliberately absent 
                          redirectUri: null,
                          state: state,
                          nonce: nonce);
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorWasCalled.Should().BeTrue();
            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Preserves_query_parameters_in_redirect_uri()
        {
            await _mockPipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _mockPipeline.CreateAuthorizeUrl(
                           clientId: "code_client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_client/callback?foo=bar&baz=quux",
                           state: state,
                           nonce: nonce);
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://code_client/callback?");
            var authorization = _mockPipeline.ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
            authorization.Code.Should().NotBeNull();
            authorization.State.Should().Be(state);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(response.Headers.Location.Query);
            query["foo"].ToString().Should().Be("bar");
            query["baz"].ToString().Should().Be("quux");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Rejects_redirect_uri_when_query_parameter_does_not_match()
        {
            await _mockPipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();

            var url = _mockPipeline.CreateAuthorizeUrl(
                           clientId: "code_client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_client/callback?baz=quux&foo=bar",
                           state: state,
                           nonce: nonce);
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorWasCalled.Should().BeTrue();
            _mockPipeline.ErrorMessage.Error.Should().Be("unauthorized_client");
        }
    }
}
