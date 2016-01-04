// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;


namespace IdentityServer4.Tests.Conformance.Basic
{
    public class ClientAuthenticationTests : AuthorizeEndpointTestBase
    {
        const string Category = "Conformance.Basic.ClientAuthenticationTests";

        public ClientAuthenticationTests()
        {
            Scopes.Add(StandardScopes.OpenId);
            Clients.Add(new Client
            {
                Enabled = true,
                ClientId = "code_client",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha512())
                },

                Flow = Flows.AuthorizationCode,
                AllowAccessToAllScopes = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    "https://code_client/callback",
                    "https://code_client/callback?foo=bar&baz=quux"
                }
            });
            Users.Add(new InMemoryUser
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
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_endpoint_supports_client_authentication_with_basic_authentication_with_POST()
        {
            await LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();

            _browser.AllowAutoRedirect = false;
            var url = CreateAuthorizeUrl(
                           clientId: "code_client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_client/callback?foo=bar&baz=quux",
                           nonce: nonce);
            var response = await _client.GetAsync(url);

            var authorization = ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
            authorization.Code.Should().NotBeNull();

            var code = authorization.Code;

            // backchannel client
            var wrapper = new MessageHandlerWrapper(_server.CreateHandler());
            var tokenClient = new TokenClient(TokenEndpoint, "code_client", "secret", wrapper);
            var tokenResult = await tokenClient.RequestAuthorizationCodeAsync(code, "https://code_client/callback?foo=bar&baz=quux");

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
            await LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();

            _browser.AllowAutoRedirect = false;
            var url = CreateAuthorizeUrl(
                           clientId: "code_client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_client/callback?foo=bar&baz=quux",
                           nonce: nonce);
            var response = await _client.GetAsync(url);

            var authorization = ParseAuthorizationResponseUrl(response.Headers.Location.ToString());
            authorization.Code.Should().NotBeNull();

            var code = authorization.Code;

            // backchannel client
            var tokenClient = new TokenClient(TokenEndpoint, "code_client", "secret", _server.CreateHandler(), AuthenticationStyle.PostValues);
            var tokenResult = await tokenClient.RequestAuthorizationCodeAsync(code, "https://code_client/callback?foo=bar&baz=quux");

            tokenResult.IsError.Should().BeFalse();
            tokenResult.IsHttpError.Should().BeFalse();
            tokenResult.TokenType.Should().Be("Bearer");
            tokenResult.AccessToken.Should().NotBeNull();
            tokenResult.ExpiresIn.Should().BeGreaterThan(0);
            tokenResult.IdentityToken.Should().NotBeNull();
        }
    }
}
