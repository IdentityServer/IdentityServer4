// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Hosting;
using IdentityServer4.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static IdentityServer4.Constants;

namespace IdentityServer.IntegrationTests.Endpoints.Custom
{
    public class CustomEndpointTest
    {
        private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();
        StubEndpointHandler _stubEndpointHandler = new StubEndpointHandler();

        [Fact]
        public async Task custom_endpoint_should_be_called()
        {
            _mockPipeline.OnPostConfigureServices += services =>
            {
                services.AddSingleton(_stubEndpointHandler);
                services.AddSingleton(new Endpoint("test", "/test", typeof(StubEndpointHandler)));
            };
            _mockPipeline.Initialize();

            var result = await _mockPipeline.Client.GetAsync(IdentityServerPipeline.BaseUrl + "/test");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            _stubEndpointHandler.ProcessAsyncWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task custom_endpoint_should_be_allowed_to_replace_built_in_endpoint()
        {
            _mockPipeline.OnPostConfigureServices += services =>
            {
                services.AddSingleton(_stubEndpointHandler);
                services.AddSingleton(new Endpoint(EndpointNames.Token, "/" + ProtocolRoutePaths.Token, typeof(StubEndpointHandler)));
            };
            _mockPipeline.Initialize();

            var result = await _mockPipeline.Client.GetAsync(IdentityServerPipeline.TokenEndpoint);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            _stubEndpointHandler.ProcessAsyncWasCalled.Should().BeTrue();
        }
    }
}
