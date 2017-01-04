﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class ExtensionGrantClient
    {
        const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public ExtensionGrantClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task Valid_Client()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("custom", "api1", customParameters);

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.custom");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("custom");
        }

        [Fact]
        public async Task Valid_Client_with_default_Scopes()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("custom", extra: customParameters);

            response.IsError.Should().BeFalse();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.custom");
            payload.Should().Contain("sub", "818727");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("custom");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(2);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("api2");
        }

        [Fact]
        public async Task Valid_Client_Missing_Grant_Specific_Data()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestCustomGrantAsync("custom", "api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
            response.ErrorDescription.Should().Be("invalid_custom_credential");
        }

        [Fact]
        public async Task Valid_Client_Unsupported_Grant()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("invalid", "api1", customParameters);

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("unsupported_grant_type");
        }

        [Fact]
        public async Task Valid_Client_Unauthorized_Grant()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "custom_credential", "custom credential"}
                };

            var response = await client.RequestCustomGrantAsync("custom2", "api1", customParameters);

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("unsupported_grant_type");
        }

        private Dictionary<string, object> GetPayload(TokenResponse response)
        {
            var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Encoding.UTF8.GetString(Base64Url.Decode(token)));

            return dictionary;
        }
    }
}