// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
    public class ClientCredentialsClient
    {
        private const string TokenEndpoint = "https://server/connect/token";

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
        public async Task Invalid_endpoint_should_return_404()
        {
            var client = new TokenClient(
                TokenEndpoint + "invalid",
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Http);
            response.Error.Should().Be("Not Found");
            response.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Valid_request_should_return_expected_payload()
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
            payload.Should().Contain("client_id", "client");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");
        }

        [Fact]
        public async Task Requesting_multiple_scopes_should_return_expected_payload()
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
            payload.Should().Contain("client_id", "client");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(2);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("api2");
        }

        [Fact]
        public async Task Request_with_no_explicit_scopes_should_return_expected_payload()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync();

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.Count().Should().Be(2);
            scopes.First().ToString().Should().Be("api1");
            scopes.Skip(1).First().ToString().Should().Be("api2");
        }

        [Fact]
        public async Task Client_without_default_scopes_skipping_scope_parameter_should_return_error()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.no_default_scopes",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync();

            response.IsError.Should().Be(true);
            response.ExpiresIn.Should().Be(0);
            response.TokenType.Should().BeNull();
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();
            response.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        }

        [Fact]
        public async Task Request_posting_client_secret_in_body_should_succeed()
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
            payload.Should().Contain("client_id", "client");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");
        }


        [Fact]
        public async Task Request_For_client_with_no_secret_and_basic_authentication_should_succeed()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.no_secret",
                "",
                _handler,
                AuthenticationStyle.BasicAuthentication);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();

            var payload = GetPayload(response);

            payload.Count().Should().Be(6);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.no_secret");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");
        }

        [Fact]
        public async Task Request_with_invalid_client_secret_should_fail()
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
        public async Task Unknown_client_should_fail()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "invalid",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_client");
        }

        [Fact]
        public async Task Implicit_client_should_not_use_client_credential_grant()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "implicit",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("unauthorized_client");
        }

        [Fact]
        public async Task Implicit_and_client_creds_client_should_not_use_client_credential_grant_without_secret()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "implicit_and_client_creds",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_client");
        }


        [Fact]
        public async Task Requesting_unknown_scope_should_fail()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("unknown");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_scope");
        }

        [Fact]
        public async Task Client_explicitly_requesting_identity_scope_should_fail()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.identityscopes",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("openid api1");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_scope");
        }

        [Fact]
        public async Task Client_explicitly_requesting_offline_access_should_fail()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1 offline_access");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_scope");
        }

        [Fact]
        public async Task Requesting_unauthorized_scope_should_fail()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api3");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Error.Should().Be("invalid_scope");
        }

        [Fact]
        public async Task Requesting_authorized_and_unauthorized_scopes_should_fail()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestClientCredentialsAsync("api1 api3");

            response.IsError.Should().Be(true);
            response.ErrorType.Should().Be(ResponseErrorType.Protocol);
            response.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
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