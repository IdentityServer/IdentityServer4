// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.IntegrationTests.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients
{
    public class DiscoveryClientTests
    {
        private const string DiscoveryEndpoint = "https://server/.well-known/openid-configuration";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public DiscoveryClientTests()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task Retrieving_discovery_document_should_succeed()
        {
            var client = new DiscoveryClient(DiscoveryEndpoint, _handler);
            var doc = await client.GetAsync();
        }

        [Fact]
        public async Task Discovery_document_should_have_expected_values()
        {
            var client = new DiscoveryClient(DiscoveryEndpoint, _handler);
            client.Policy.ValidateIssuerName = false;

            var doc = await client.GetAsync();

            // endpoints
            doc.TokenEndpoint.Should().Be("https://server/connect/token");
            doc.AuthorizeEndpoint.Should().Be("https://server/connect/authorize");
            doc.IntrospectionEndpoint.Should().Be("https://server/connect/introspect");
            doc.EndSessionEndpoint.Should().Be("https://server/connect/endsession");

            // jwk
            doc.KeySet.Keys.Count.Should().Be(1);
            doc.KeySet.Keys.First().E.Should().NotBeNull();
            doc.KeySet.Keys.First().N.Should().NotBeNull();
        }
    }
}