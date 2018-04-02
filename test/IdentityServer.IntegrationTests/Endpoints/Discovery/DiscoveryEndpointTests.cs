// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.IntegrationTests.Common;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Endpoints.Discovery
{
    public class DiscoveryEndpointTests
    {
        private const string Category = "Discovery endpoint";

        [Fact]
        [Trait("Category", Category)]
        public async Task issuer_uri_should_be_lowercase()
        {
            IdentityServerPipeline pipeline = new IdentityServerPipeline();
            pipeline.Initialize("/ROOT");

            var result = await pipeline.Client.GetAsync("HTTPS://SERVER/ROOT/.WELL-KNOWN/OPENID-CONFIGURATION");

            var json = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var issuer = data["issuer"].ToString();

            issuer.Should().Be("https://server/root");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task jwks_entries_should_contain_alg()
        {
            IdentityServerPipeline pipeline = new IdentityServerPipeline();
            pipeline.Initialize("/ROOT");

            var result = await pipeline.Client.GetAsync("https://server/root/.well-known/openid-configuration/jwks");

            var json = await result.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var keys = data["keys"];
            keys.Should().NotBeNull();

            var key = keys[0];
            key.Should().NotBeNull();

            var alg = key["alg"];
            alg.Should().NotBeNull();

            alg.Value<string>().Should().Be(Constants.SigningAlgorithms.RSA_SHA_256);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task unicode_values_in_url_should_be_processed_correctly()
        {
            var pipeline = new IdentityServerPipeline();
            pipeline.Initialize();

            var discoClient = new DiscoveryClient("https://грант.рф", pipeline.Handler);
            discoClient.Policy.ValidateIssuerName = false;
            discoClient.Policy.ValidateEndpoints = false;
            discoClient.Policy.RequireHttps = false;
            discoClient.Policy.RequireKeySet = false;

            var result = await discoClient.GetAsync();
            result.Issuer.Should().Be("https://грант.рф");
        }
    }
}
