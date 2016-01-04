// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Conformance.Basic
{
    public class ResponseTypeResponseModeTests : AuthorizeEndpointTestBase
    {
        const string Category = "Conformance.Basic.ResponseTypeResponseModeTests";

        public ResponseTypeResponseModeTests()
        {
            _browser.AllowAutoRedirect = false;

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
                    "https://code_client/callback"
                }
            });

            Scopes.Add(StandardScopes.OpenId);

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
        public async Task Request_with_response_type_code_supported()
        {
            await LoginAsync("bob");

            var metadata = await _client.GetAsync(DiscoveryEndpoint);
            metadata.StatusCode.Should().Be(HttpStatusCode.OK);

            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            var url = CreateAuthorizeUrl(
                           clientId: "code_client",
                           responseType: "code",
                           scope: "openid",
                           redirectUri: "https://code_client/callback",
                           state: state,
                           nonce: nonce);
            var response = await _client.GetAsync(url);
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeFalse();
            authorization.Code.Should().NotBeNull();
            authorization.State.Should().Be(state);
        }

        // todo brock: update to new behavior
        //[Fact]
        //[Trait("Category", Category)]
        //public void Request_missing_response_type_rejected()
        //{
        //    host.Login();

        //    var state = Guid.NewGuid().ToString();
        //    var nonce = Guid.NewGuid().ToString();

        //    var url = host.GetAuthorizeUrl(client_id, redirect_uri, "openid", /*response_type*/ null, state, nonce);

        //    var result = host.Client.GetAsync(url).Result;
        //    result.StatusCode.Should().Be(HttpStatusCode.Found);
        //    result.Headers.Location.AbsoluteUri.Should().Contain("#");

        //    var query = result.Headers.Location.ParseHashFragment();
        //    query.AllKeys.Should().Contain("state");
        //    query["state"].Should().Be(state);
        //    query.AllKeys.Should().Contain("error");
        //    query["error"].Should().Be("unsupported_response_type");
        //}
    }
}
