using FluentAssertions;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.CheckSession
{
    public class CheckSessionTests
    {
        const string Category = "Check session endpoint";

        MockIdSvrUiPipeline _mockPipeline = new MockIdSvrUiPipeline();

        public CheckSessionTests()
        {
            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _mockPipeline.Client.GetAsync(MockIdSvrUiPipeline.CheckSessionEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }
    }
}
