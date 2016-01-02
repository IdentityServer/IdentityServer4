using Microsoft.AspNet.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Threading;
using System;
using IdentityServer4.Core.Extensions;
using IdentityModel.Client;
using IdentityServer4.Tests.Common;

namespace IdentityServer4.Tests.Endpoints.Authorize
{
    public class AuthorizeTests
    {
        const string Category = "Authorize endpoint";
        const string Endpoint = "https://server/connect/authorize";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;
        private readonly Startup _startup = new Startup();
        private readonly AuthorizeRequest _authorizeRequest = new AuthorizeRequest(Endpoint);

        public AuthorizeTests()
        {
            var server = TestServer.Create(null, _startup.Configure, _startup.ConfigureServices);
            _handler = server.CreateHandler();
            _client = new HttpClient(new BrowsR(_handler));
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

            ((int)response.StatusCode).Should().BeLessThan(500);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task anonymous_user_is_redirected_to_login_page()
        {
            var url = _authorizeRequest.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri:"https://client1/callback",
                state:  "123_state",
                nonce: "123_nonce");
            var response = await _client.GetAsync(url);

            _startup.LoginWasCalled.Should().BeTrue();
        }
    }
}
