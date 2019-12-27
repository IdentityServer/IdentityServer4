// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients
{
    public class UserInfoEndpointClient
    {
        private const string TokenEndpoint = "https://server/connect/token";
        private const string UserInfoEndpoint = "https://server/connect/userinfo";

        private readonly HttpClient _client;

        public UserInfoEndpointClient()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            var server = new TestServer(builder);

            _client = server.CreateClient();
        }

        [Fact]
        public async Task Valid_client_with_GET_should_succeed()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "openid email api1",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();

            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = UserInfoEndpoint,
                Token = response.AccessToken
            });

            userInfo.IsError.Should().BeFalse();
            userInfo.Claims.Count().Should().Be(3);

            userInfo.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "88421113");
            userInfo.Claims.Should().Contain(c => c.Type == "email" && c.Value == "BobSmith@email.com");
            userInfo.Claims.Should().Contain(c => c.Type == "email_verified" && c.Value == "true");
        }

        [Fact]
        public async Task Request_address_scope_should_return_expected_response()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "openid address",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();

            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = UserInfoEndpoint,
                Token = response.AccessToken
            });

            userInfo.IsError.Should().BeFalse();
            userInfo.Raw.Should().Be("{\"address\":{\"street_address\":\"One Hacker Way\",\"locality\":\"Heidelberg\",\"postal_code\":69118,\"country\":\"Germany\"},\"sub\":\"88421113\"}");
        }

        [Fact]
        public async Task Using_a_token_with_no_identity_scope_should_fail()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "api1",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();

            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = UserInfoEndpoint,
                Token = response.AccessToken
            });

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Using_a_token_with_an_identity_scope_but_no_openid_should_fail()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "email api1",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();

            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = UserInfoEndpoint,
                Token = response.AccessToken
            });

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Invalid_token_should_fail()
        {
            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = UserInfoEndpoint,
                Token = "invalid"
            });

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Complex_json_should_be_correct()
        {
            var response = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenEndpoint,
                ClientId = "roclient",
                ClientSecret = "secret",

                Scope = "openid email api1 api4.with.roles roles",
                UserName = "bob",
                Password = "bob"
            });

            response.IsError.Should().BeFalse();
            
            var payload = GetPayload(response);

            var scopes = ((JArray)payload["scope"]).Select(x => x.ToString()).ToArray();
            scopes.Length.Should().Be(5);
            scopes.Should().Contain("openid");
            scopes.Should().Contain("email");
            scopes.Should().Contain("api1");
            scopes.Should().Contain("api4.with.roles");
            scopes.Should().Contain("roles");

            var roles = ((JArray)payload["role"]).Select(x => x.ToString()).ToArray();
            roles.Length.Should().Be(2);
            roles.Should().Contain("Geek");
            roles.Should().Contain("Developer");

            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = UserInfoEndpoint,
                Token = response.AccessToken
            });

            roles = ((JArray)userInfo.Json["role"]).Select(x => x.ToString()).ToArray();
            roles.Length.Should().Be(2);
            roles.Should().Contain("Geek");
            roles.Should().Contain("Developer");
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