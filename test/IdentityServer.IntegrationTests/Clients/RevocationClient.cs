// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class RevocationClient
    {
        private const string TokenEndpoint = "https://server/connect/token";
        private const string RevocationEndpoint = "https://server/connect/revocation";
        private const string IntrospectionEndpoint = "https://server/connect/introspect";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public RevocationClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task Revoking_reference_token_should_invalidate_token()
        {
            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api",
                "secret",
                innerHttpMessageHandler: _handler);

            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient.reference",
                "secret",
                innerHttpMessageHandler: _handler);

            var revocationClient = new TokenRevocationClient(
                RevocationEndpoint,
                "roclient.reference",
                "secret",
                innerHttpMessageHandler: _handler);

            // request acccess token
            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");
            response.IsError.Should().BeFalse();

            // introspect - should be active
            var introspectionResponse = await introspectionClient.SendAsync(
                new IntrospectionRequest { Token = response.AccessToken });
            introspectionResponse.IsActive.Should().Be(true);

            // revoke access token
            var revocationResponse = await revocationClient.RevokeAccessTokenAsync(response.AccessToken);

            // introspect - should be inactive
            introspectionResponse = await introspectionClient.SendAsync(
                new IntrospectionRequest { Token = response.AccessToken });
            introspectionResponse.IsActive.Should().Be(false);

        }

    }
}