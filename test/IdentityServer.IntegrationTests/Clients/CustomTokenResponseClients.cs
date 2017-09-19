// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Clients
{
    public class CustomTokenResponseClients
    {
        private const string TokenEndpoint = "https://server/connect/token";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public CustomTokenResponseClients()
        {
            var builder = new WebHostBuilder()
                .UseStartup<StartupWithCustomTokenResponses>();
            var server = new TestServer(builder);

            _handler = server.CreateHandler();
            _client = server.CreateClient();
        }

        [Fact]
        public async Task resource_owner_success_should_return_custom_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");


            // raw fields
            var fields = GetFields(response);
            fields.Should().Contain("string_value", "some_string");
            ((Int64)fields["int_value"]).Should().Be(42);

            object temp;
            fields.TryGetValue("identity_token", out temp).Should().BeFalse();
            fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
            fields.TryGetValue("error", out temp).Should().BeFalse();
            fields.TryGetValue("error_description", out temp).Should().BeFalse();
            fields.TryGetValue("token_type", out temp).Should().BeTrue();
            fields.TryGetValue("expires_in", out temp).Should().BeTrue();

            var responseObject = fields["dto"] as JObject;
            responseObject.Should().NotBeNull();

            var responseDto = GetDto(responseObject);
            var dto = CustomResponseDto.Create;

            responseDto.string_value.Should().Be(dto.string_value);
            responseDto.int_value.Should().Be(dto.int_value);
            responseDto.nested.string_value.Should().Be(dto.nested.string_value);
            responseDto.nested.int_value.Should().Be(dto.nested.int_value);


            // token client response
            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();
            

            // token content
            var payload = GetPayload(response);
            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "roclient");
            payload.Should().Contain("sub", "bob");
            payload.Should().Contain("idp", "local");

            var audiences = ((JArray)payload["aud"]).Select(x => x.ToString());
            audiences.Count().Should().Be(2);
            audiences.Should().Contain("https://idsvr4/resources");
            audiences.Should().Contain("api");

            var scopes = payload["scope"] as JArray;
            scopes.First().ToString().Should().Be("api1");

            var amr = payload["amr"] as JArray;
            amr.Count().Should().Be(1);
            amr.First().ToString().Should().Be("password");
        }

        [Fact]
        public async Task resource_owner_failure_should_return_custom_error_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await client.RequestResourceOwnerPasswordAsync("bob", "invalid", "api1");


            // raw fields
            var fields = GetFields(response);
            fields.Should().Contain("string_value", "some_string");
            ((Int64)fields["int_value"]).Should().Be(42);

            object temp;
            fields.TryGetValue("identity_token", out temp).Should().BeFalse();
            fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
            fields.TryGetValue("error", out temp).Should().BeTrue();
            fields.TryGetValue("error_description", out temp).Should().BeTrue();
            fields.TryGetValue("token_type", out temp).Should().BeFalse();
            fields.TryGetValue("expires_in", out temp).Should().BeFalse();

            var responseObject = fields["dto"] as JObject;
            responseObject.Should().NotBeNull();

            var responseDto = GetDto(responseObject);
            var dto = CustomResponseDto.Create;

            responseDto.string_value.Should().Be(dto.string_value);
            responseDto.int_value.Should().Be(dto.int_value);
            responseDto.nested.string_value.Should().Be(dto.nested.string_value);
            responseDto.nested.int_value.Should().Be(dto.nested.int_value);


            // token client response
            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_grant");
            response.ErrorDescription.Should().Be("invalid_credential");
            response.ExpiresIn.Should().Be(0);
            response.TokenType.Should().BeNull();
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();
        }

        [Fact]
        public async Task extension_grant_success_should_return_custom_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "outcome", "succeed"}
                };

            var response = await client.RequestCustomGrantAsync("custom", "api1", customParameters);

            // raw fields
            var fields = GetFields(response);
            fields.Should().Contain("string_value", "some_string");
            ((Int64)fields["int_value"]).Should().Be(42);

            object temp;
            fields.TryGetValue("identity_token", out temp).Should().BeFalse();
            fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
            fields.TryGetValue("error", out temp).Should().BeFalse();
            fields.TryGetValue("error_description", out temp).Should().BeFalse();
            fields.TryGetValue("token_type", out temp).Should().BeTrue();
            fields.TryGetValue("expires_in", out temp).Should().BeTrue();

            var responseObject = fields["dto"] as JObject;
            responseObject.Should().NotBeNull();

            var responseDto = GetDto(responseObject);
            var dto = CustomResponseDto.Create;

            responseDto.string_value.Should().Be(dto.string_value);
            responseDto.int_value.Should().Be(dto.int_value);
            responseDto.nested.string_value.Should().Be(dto.nested.string_value);
            responseDto.nested.int_value.Should().Be(dto.nested.int_value);


            // token client response
            response.IsError.Should().Be(false);
            response.ExpiresIn.Should().Be(3600);
            response.TokenType.Should().Be("Bearer");
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();


            // token content
            var payload = GetPayload(response);
            payload.Count().Should().Be(10);
            payload.Should().Contain("iss", "https://idsvr4");
            payload.Should().Contain("client_id", "client.custom");
            payload.Should().Contain("sub", "bob");
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
        public async Task extension_grant_failure_should_return_custom_error_response()
        {
            var client = new TokenClient(
                TokenEndpoint,
                "client.custom",
                "secret",
                innerHttpMessageHandler: _handler);

            var customParameters = new Dictionary<string, string>
                {
                    { "outcome", "fail"}
                };

            var response = await client.RequestCustomGrantAsync("custom", "api1", customParameters);

            // raw fields
            var fields = GetFields(response);
            fields.Should().Contain("string_value", "some_string");
            ((Int64)fields["int_value"]).Should().Be(42);

            object temp;
            fields.TryGetValue("identity_token", out temp).Should().BeFalse();
            fields.TryGetValue("refresh_token", out temp).Should().BeFalse();
            fields.TryGetValue("error", out temp).Should().BeTrue();
            fields.TryGetValue("error_description", out temp).Should().BeTrue();
            fields.TryGetValue("token_type", out temp).Should().BeFalse();
            fields.TryGetValue("expires_in", out temp).Should().BeFalse();

            var responseObject = fields["dto"] as JObject;
            responseObject.Should().NotBeNull();

            var responseDto = GetDto(responseObject);
            var dto = CustomResponseDto.Create;

            responseDto.string_value.Should().Be(dto.string_value);
            responseDto.int_value.Should().Be(dto.int_value);
            responseDto.nested.string_value.Should().Be(dto.nested.string_value);
            responseDto.nested.int_value.Should().Be(dto.nested.int_value);


            // token client response
            response.IsError.Should().Be(true);
            response.Error.Should().Be("invalid_grant");
            response.ErrorDescription.Should().Be("invalid_credential");
            response.ExpiresIn.Should().Be(0);
            response.TokenType.Should().BeNull();
            response.IdentityToken.Should().BeNull();
            response.RefreshToken.Should().BeNull();
        }

        private CustomResponseDto GetDto(JObject responseObject)
        {
            return responseObject.ToObject<CustomResponseDto>();
        }

        private Dictionary<string, object> GetFields(TokenResponse response)
        {
            return response.Json.ToObject<Dictionary<string, object>>();
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