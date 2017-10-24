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
    public class UserInfoEndpointClient
    {
        private const string TokenEndpoint = "https://server/connect/token";
        private const string UserInfoEndpoint = "https://server/connect/userinfo";

        private readonly HttpClient _client;
        private readonly HttpMessageHandler _handler;

        public UserInfoEndpointClient()
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
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                UserInfoEndpoint,
                _handler);

            var userInfo = await userInfoclient.GetAsync(response.AccessToken);

            userInfo.IsError.Should().BeFalse();
            userInfo.Claims.Count().Should().Be(3);

            userInfo.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "88421113");
            userInfo.Claims.Should().Contain(c => c.Type == "email" && c.Value == "BobSmith@email.com");
            userInfo.Claims.Should().Contain(c => c.Type == "email_verified" && c.Value == "True");
        }

        [Fact]
        public async Task Address_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "openid address");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                UserInfoEndpoint,
                _handler);

            var userInfo = await userInfoclient.GetAsync(response.AccessToken);

            userInfo.IsError.Should().BeFalse();
            userInfo.Raw.Should().Be("{\"address\":{\"street_address\":\"One Hacker Way\",\"locality\":\"Heidelberg\",\"postal_code\":69118,\"country\":\"Germany\"},\"sub\":\"88421113\"}");
        }

        [Fact]
        public async Task No_Identity_Scope()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "api1");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                UserInfoEndpoint,
                _handler);

            var userInfo = await userInfoclient.GetAsync(response.AccessToken);

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Identity_Scope_No_OpenID()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "email api1");
            response.IsError.Should().BeFalse();

            var userInfoclient = new UserInfoClient(
                UserInfoEndpoint,
                _handler);

            var userInfo = await userInfoclient.GetAsync(response.AccessToken);

            userInfo.IsError.Should().BeTrue();
            userInfo.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task json_should_be_correct()
        {
            var tokenClient = new TokenClient(
                TokenEndpoint,
                "roclient",
                "secret",
                innerHttpMessageHandler: _handler);

            var response = await tokenClient.RequestResourceOwnerPasswordAsync("bob", "bob", "openid email api1 api4.with.roles roles");
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

            var userInfoclient = new UserInfoClient(
                UserInfoEndpoint,
                _handler);

            var userInfo = await userInfoclient.GetAsync(response.AccessToken);

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