﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;


namespace IdentityServer4.Tests.Conformance.Basic
{
    public class ClientAuthenticationTests 
    {
        const string Category = "Conformance.Basic.ClientAuthenticationTests";

        MockIdSvrUiPipeline _pipeline = new MockIdSvrUiPipeline();

        public ClientAuthenticationTests()
        {
            _pipeline.Scopes.Add(StandardScopes.OpenId);
            _pipeline.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = "code_pipeline.Client",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha512())
                },

                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessToAllScopes = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    "https://code_pipeline.Client/callback",
                    "https://code_pipeline.Client/callback?foo=bar&baz=quux"
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

            _pipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_endpoint_supports_client_authentication_with_basic_authentication_with_POST()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();

            _pipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _pipeline.CreateAuthorizeUrl(
                           clientId: "code_pipeline.Client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_pipeline.Client/callback?foo=bar&baz=quux",
                           nonce: nonce);
            var response = await _pipeline.BrowserClient.GetAsync(url);

            var authorization = _pipeline.ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
            authorization.Code.Should().NotBeNull();

            var code = authorization.Code;

            // backchannel client
            var wrapper = new MessageHandlerWrapper(_pipeline.Handler);
            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, "code_pipeline.Client", "secret", wrapper);
            var tokenResult = await tokenClient.RequestAuthorizationCodeAsync(code, "https://code_pipeline.Client/callback?foo=bar&baz=quux");

            tokenResult.IsError.Should().BeFalse();
            tokenResult.IsHttpError.Should().BeFalse();
            tokenResult.TokenType.Should().Be("Bearer");
            tokenResult.AccessToken.Should().NotBeNull();
            tokenResult.ExpiresIn.Should().BeGreaterThan(0);
            tokenResult.IdentityToken.Should().NotBeNull();

            wrapper.Response.Headers.CacheControl.NoCache.Should().BeTrue();
            wrapper.Response.Headers.CacheControl.NoStore.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_endpoint_supports_client_authentication_with_form_encoded_authentication_in_POST_body()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();

            _pipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _pipeline.CreateAuthorizeUrl(
                           clientId: "code_pipeline.Client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_pipeline.Client/callback?foo=bar&baz=quux",
                           nonce: nonce);
            var response = await _pipeline.BrowserClient.GetAsync(url);

            var authorization = _pipeline.ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
            authorization.Code.Should().NotBeNull();

            var code = authorization.Code;

            // backchannel client
            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, "code_pipeline.Client", "secret", _pipeline.Handler, AuthenticationStyle.PostValues);
            var tokenResult = await tokenClient.RequestAuthorizationCodeAsync(code, "https://code_pipeline.Client/callback?foo=bar&baz=quux");

            tokenResult.IsError.Should().BeFalse();
            tokenResult.IsHttpError.Should().BeFalse();
            tokenResult.TokenType.Should().Be("Bearer");
            tokenResult.AccessToken.Should().NotBeNull();
            tokenResult.ExpiresIn.Should().BeGreaterThan(0);
            tokenResult.IdentityToken.Should().NotBeNull();
        }
    }
}
