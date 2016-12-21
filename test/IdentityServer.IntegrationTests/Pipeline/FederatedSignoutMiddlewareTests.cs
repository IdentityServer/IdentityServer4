// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Test;
using IdentityServer4.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Pipeline
{
    public class FederatedSignoutMiddlewareTests
    {
        const string Category = "CORS Integration";

        MockIdSvrUiPipeline _pipeline = new MockIdSvrUiPipeline();
        ClaimsPrincipal _user;

        string _idSvrSid;
        string _idSvrIFrameUrl;

        public FederatedSignoutMiddlewareTests()
        {
            _user = IdentityServerPrincipal.Create("bob", "bob", new Claim(JwtClaimTypes.SessionId, "123"));
            _pipeline = new MockIdSvrUiPipeline();

            _pipeline.Users.Add(new TestUser
            {
                SubjectId = "bob",
                Username = "bob",
                Claims = new Claim[]
               {
                    new Claim("name", "Bob Loblaw"),
                    new Claim("email", "bob@loblaw.com"),
                    new Claim("role", "Attorney"),
               }
            });

            _pipeline.FederatedSignOut = async ctx =>
            {
                _idSvrIFrameUrl = await ctx.GetIdentityServerSignoutFrameCallbackUrlAsync();
                ISessionIdService sessionId = ctx.RequestServices.GetRequiredService<ISessionIdService>();
                _idSvrSid = await sessionId.GetCurrentSessionIdAsync();
            };
            _pipeline.Initialize();
            _pipeline.Options.Authentication.FederatedSignOutPaths.Add(MockIdSvrUiPipeline.FederatedSignOutPath);
        }

        [Fact]
        public async Task valid_request_to_federated_signout_endpoint_should_render_page_with_iframe()
        {
            await _pipeline.LoginAsync(_user);

            var response = await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl + "?sid=123");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/html");
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain(_idSvrIFrameUrl);
        }

        [Fact]
        public async Task valid_POST_request_to_federated_signout_endpoint_should_render_page_with_iframe()
        {
            await _pipeline.LoginAsync(_user);

            var response = await _pipeline.BrowserClient.PostAsync(MockIdSvrUiPipeline.FederatedSignOutUrl, new FormUrlEncodedContent(new Dictionary<string, string> { { "sid", "123" } }));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/html");
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain(_idSvrIFrameUrl);
        }

        [Fact]
        public async Task valid_request_to_federated_signout_endpoint_should_sign_user_out()
        {
            await _pipeline.LoginAsync(_user);

            var response = await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl + "?sid=123");

            _pipeline.FederatedSignOut = async ctx =>
            {
                var user = await ctx.GetIdentityServerUserAsync();
                user.Should().BeNull();
            };
            await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl);
        }

        [Fact]
        public async Task no_signout_paths_configured_should_not_render_page_with_iframe()
        {
            _pipeline.Options.Authentication.FederatedSignOutPaths.Clear();

            await _pipeline.LoginAsync(_user);

            var response = await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl + "?sid=123");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.Should().BeNull();
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Be(String.Empty);
        }

        [Fact]
        public async Task no_authenticated_user_should_not_render_page_with_iframe()
        {
            var response = await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl + "?sid=123");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.Should().BeNull();
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Be(String.Empty);
        }

        [Fact]
        public async Task authenticated_user_sid_does_not_match_param_should_not_render_page_with_iframe()
        {
            await _pipeline.LoginAsync(_user);

            var response = await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl + "?sid=456");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.Should().BeNull();
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Be(String.Empty);
        }


        [Fact]
        public async Task non_200_should_not_render_page_with_iframe()
        {
            _pipeline.FederatedSignOut = ctx =>
            {
                ctx.Response.StatusCode = 404;
                return Task.FromResult(0);
            };
            await _pipeline.LoginAsync(_user);

            var response = await _pipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.FederatedSignOutUrl + "?sid=123");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType.Should().BeNull();
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Be(String.Empty);
        }
    }
}
