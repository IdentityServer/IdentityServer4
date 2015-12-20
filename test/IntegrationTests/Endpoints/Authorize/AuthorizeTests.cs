using Microsoft.AspNet.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using IdentityModel.Client;
using System.Linq;

namespace IdentityServer4.Tests.Endpoints.Authorize
{
    public class AuthorizeTests
    {
        const string Category = "Authorize endpoint";
        const string Endpoint = "https://server/connect/authorize";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public AuthorizeTests()
        {
            var server = new TestServer(TestServer.CreateBuilder()
                                .UseStartup<Startup>());

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _client.GetAsync(Endpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_500()
        {
            var response = await _client.GetAsync(Endpoint);

            (((int)response.StatusCode) < 500).Should().Be(true);
        }
    }
}
