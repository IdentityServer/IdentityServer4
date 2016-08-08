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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Clients
{
    public class ClientCredentialsClient
    {
        const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public ClientCredentialsClient()
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
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("aud", "https://idsvr4/resources");
            payload.Should().Contain("client_id", "client");
            payload.Should().Contain("scope", "api1");
        }

        [Fact]
        public async Task Valid_Client_Multiple_Scopes()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1 api2");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("aud", "https://idsvr4/resources");
            payload.Should().Contain("client_id", "client");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(2);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("api2");
        }

        [Fact]
        public async Task Valid_Client_PostBody()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                _handler,
                AuthenticationStyle.PostValues);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("aud", "https://idsvr4/resources");
            payload.Should().Contain("client_id", "client");
            payload.Should().Contain("scope", "api1");
        }

        [Fact]
        public async Task Invalid_Client_Secret()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "invalid",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_client");
        }

        [Fact]
        public async Task Invalid_Client()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "invalid",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_client");
        }

        [Fact]
        public async Task Unknown_Scope()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("unknown");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_scope");
        }

        [Fact]
        public async Task UnauthorizedScope()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api3");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_scope");
        }

        [Fact]
        public async Task Authorized_and_UnauthorizedScope()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1 api3");

            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_scope");
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