// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using IdentityModel.Client;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace IdentityServer4.IntegrationTests.Endpoints.Introspection
{
    public class IntrospectionTests
    {
        private const string Category = "Introspection endpoint";
        private const string IntrospectionEndpoint = "https://server/connect/introspect";
        private const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public IntrospectionTests()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Request()
        {
            var form = new Dictionary<string, string>();

            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope()
        {
            var form = new Dictionary<string, string>();

            _client.SetBasicAuthentication("unknown", "invalid");
            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_ScopeSecret()
        {
            var form = new Dictionary<string, string>();

            _client.SetBasicAuthentication("api1", "invalid");
            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_Token()
        {
            var form = new Dictionary<string, string>();

            _client.SetBasicAuthentication("api1", "secret");
            var response = await _client.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Token()
        {
            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = "invalid"
            });

            response.IsActive.Should().Be(false);
            response.IsError.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_ContentType()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            var data = new
            {
                client_id = "api1",
                client_secret = "secret",
                token = tokenResponse.AccessToken
            };
            var json = JsonConvert.SerializeObject(data);

            var client = new HttpClient(_handler);
            var response = await client.PostAsync(IntrospectionEndpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Valid_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(true);
            response.IsError.Should().Be(false);

            var scopes = from c in response.Claims
                         where c.Type == "scope"
                         select c;

            scopes.Count().Should().Be(1);
            scopes.First().Value.Should().Be("api1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Response_data_should_be_valid_using_single_scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            var values = response.Json.ToObject<Dictionary<string, object>>();

            values["aud"].GetType().Name.Should().Be("JArray");

            var audiences = ((JArray)values["aud"]);
            foreach (var aud in audiences)
            {
                aud.Type.Should().Be(JTokenType.String);
            }

            values["iss"].GetType().Name.Should().Be("String");
            values["nbf"].GetType().Name.Should().Be("Int64");
            values["exp"].GetType().Name.Should().Be("Int64");
            values["client_id"].GetType().Name.Should().Be("String");
            values["active"].GetType().Name.Should().Be("Boolean");
            values["scope"].GetType().Name.Should().Be("String");

            values["scope"].ToString().Should().Be("api1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Response_data_with_user_authentication_should_be_valid_using_single_scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "ro.client",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");
            tokenResponse.IsError.Should().BeFalse();

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            var values = response.Json.ToObject<Dictionary<string, object>>();

            values["aud"].GetType().Name.Should().Be("JArray");

            var audiences = ((JArray)values["aud"]);
            foreach (var aud in audiences)
            {
                aud.Type.Should().Be(JTokenType.String);
            }

            values["iss"].GetType().Name.Should().Be("String");
            values["nbf"].GetType().Name.Should().Be("Int64");
            values["exp"].GetType().Name.Should().Be("Int64");
            values["auth_time"].GetType().Name.Should().Be("Int64");
            values["client_id"].GetType().Name.Should().Be("String");
            values["sub"].GetType().Name.Should().Be("String");
            values["active"].GetType().Name.Should().Be("Boolean");
            values["scope"].GetType().Name.Should().Be("String");

            values["scope"].ToString().Should().Be("api1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Response_data_should_be_valid_using_multiple_scopes()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api3-a api3-b");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api3",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            var values = response.Json.ToObject<Dictionary<string, object>>();

            values["aud"].GetType().Name.Should().Be("JArray");

            var audiences = ((JArray)values["aud"]);
            foreach(var aud in audiences)
            {
                aud.Type.Should().Be(JTokenType.String);
            }

            values["iss"].GetType().Name.Should().Be("String"); 
            values["nbf"].GetType().Name.Should().Be("Int64"); 
            values["exp"].GetType().Name.Should().Be("Int64"); 
            values["client_id"].GetType().Name.Should().Be("String"); 
            values["active"].GetType().Name.Should().Be("Boolean"); 
            values["scope"].GetType().Name.Should().Be("String");

            var scopes = values["scope"].ToString();
            scopes.Should().Be("api3-a api3-b");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Many_Scopes_Api_Only_See_Its_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client3",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1 api2 api3-a");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api3",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(true);
            response.IsError.Should().Be(false);

            var scopes = from c in response.Claims
                         where c.Type == "scope"
                         select c.Value;

            scopes.Count().Should().Be(1);
            scopes.First().Should().Be("api3-a");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Valid_Scope_Multiple()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1 api2");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api1",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(true);
            response.IsError.Should().Be(false);

            var scopes = from c in response.Claims
                         where c.Type == "scope"
                         select c;

            scopes.Count().Should().Be(1);
            scopes.First().Value.Should().Be("api1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_Invalid_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "client1",
                "secret",
                _handler);

            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            var introspectionClient = new IntrospectionClient(
                IntrospectionEndpoint,
                "api2",
                "secret",
                _handler);

            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = tokenResponse.AccessToken
            });

            response.IsActive.Should().Be(false);
            response.IsError.Should().Be(false);
        }
    }
}
