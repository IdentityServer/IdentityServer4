using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.DeviceAuthorization
{
    public class DeviceAuthorizationTests
    {
        private const string Category = "Device authorization endpoint";

        private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();

        public DeviceAuthorizationTests()
        {
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client1",
                ClientSecrets = {new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.DeviceFlow,
                AllowedScopes = {"openid"}
            });

            _mockPipeline.IdentityScopes.AddRange(new IdentityResource[] {
                new IdentityResources.OpenId()
            });

            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_should_return_405()
        {
            var response = await _mockPipeline.Client.GetAsync(IdentityServerPipeline.DeviceAuthorization);
            response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task empty_request()
        {
            var form = new Dictionary<string, string>();
            var response = await _mockPipeline.Client.PostAsync(IdentityServerPipeline.DeviceAuthorization, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_using_secret()
        {
            var form = new Dictionary<string, string>
            {
                {"client_id", "client1"},
                {"client_secret", "secret" }
            };
            var response = await _mockPipeline.Client.PostAsync(IdentityServerPipeline.DeviceAuthorization, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}