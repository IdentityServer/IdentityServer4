using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients
{
    public class EndpointRoutingClient
    {
        private readonly HttpClient _client;
        public EndpointRoutingClient()
        {
            var builder = new WebHostBuilder()
              .UseStartup<StartupWithEndpointRouting>();
            var server = new TestServer(builder);

            _client = server.CreateClient();
        }
        [Fact]
        public async Task DiscoveryDocumentEndpoint_shuold_success()
        {
            var response = await _client.GetDiscoveryDocumentAsync("https://idsvr4");

            response.IsError.Should().Be(false);
        }
    }
}
