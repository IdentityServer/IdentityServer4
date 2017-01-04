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
    public class ResourceOwnerClient
    {
        const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public ResourceOwnerClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task Valid_User()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = ((JArray)payload["scope"]).Select(x => x.ToString());
            scopes.Count().Should().Be(1);
            scopes.Should().Contain("api1");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("pwd");
        }

        [Fact]
        public async Task Valid_User_with_Default_Scopes()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(11);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("pwd");

            var scopes = ((JArray)payload["scope"]).Select(x => x.ToString());
            scopes.Count().Should().Be(8);

            // {[  "address",  "api1",  "api2", "api4.with.roles", "email",  "offline_access",  "openid", "role"]}

            scopes.Should().Contain("address");
            scopes.Should().Contain("api1");
            scopes.Should().Contain("api2");
            scopes.Should().Contain("api4.with.roles");
            scopes.Should().Contain("email");
            scopes.Should().Contain("offline_access");
            scopes.Should().Contain("openid");
            scopes.Should().Contain("roles");
        }

        [Fact]
        public async Task Valid_User_IdentityScopes()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("pwd");

            var scopes = ((JArray)payload["scope"]).Select(x=>x.ToString());
            scopes.Count().Should().Be(3);
            scopes.Should().Contain("api1");
            scopes.Should().Contain("email");
            scopes.Should().Contain("openid");
        }

        [Fact]
        public async Task Valid_User_IdentityScopesRefreshToken()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1 offline_access");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().NotBeNullOrWhiteSpace();

            var payload = GetPayload(response);

            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "88421113");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("pwd");

            var scopes = ((JArray)payload["scope"]).Select(x => x.ToString());
            scopes.Count().Should().Be(4);
            scopes.Should().Contain("api1");
            scopes.Should().Contain("email");
            scopes.Should().Contain("offline_access");
            scopes.Should().Contain("openid");
        }

        [Fact]
        public async Task Unknown_User()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("unknown", "bob", "api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_grant");
        }

        [Fact]
        public async Task Invalid_Password()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "invalid", "api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_grant");
        }


        private Dictionary<string, object> GetPayload(IdentityModel.Client.TokenResponse response)
        {
            var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Encoding.UTF8.GetString(Base64Url.Decode(token)));

            return dictionary;
        }
    }
}