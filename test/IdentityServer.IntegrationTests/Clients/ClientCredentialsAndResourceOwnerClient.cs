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
    public class ClientCredentialsandResourceOwnerClient
    {
        private const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public ClientCredentialsandResourceOwnerClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task resource_scope_should_be_requestable_via_client_credentials()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.and.ro",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(false);
        }

        [Fact]
        public async Task openid_scope_should_not_be_requestable_via_client_credentials()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.and.ro",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("openid api1");

            response.IsError.Should().Be(true);
        }

        [Fact]
        public async Task openid_scope_should_be_requestable_via_password()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.and.ro",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid");

            response.IsError.Should().Be(false);
        }

        [Fact]
        public async Task openid_and_resource_scope_should_be_requestable_via_password()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.and.ro",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid api1");

            response.IsError.Should().Be(false);
        }

    }
}