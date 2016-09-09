// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.IntegrationTests.Common;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Endpoints.Discovery
{
    public class DiscoveryEndpointTests
    {
        const string Category = "Discovery endpoint";

        [Fact]
        [Trait("Category", Category)]
        public async Task issuer_uri_should_be_lowercase()
        {
            MockIdSvrUiPipeline pipeline = new MockIdSvrUiPipeline();
            pipeline.Initialize("/ROOT");

            var result = await pipeline.Client.GetAsync("HTTPS://SERVER/ROOT/.WELL-KNOWN/OPENID-CONFIGURATION");

            var json = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var issuer = data["issuer"].ToString();

            issuer.Should().Be("https://server/root");
        }
    }
}
