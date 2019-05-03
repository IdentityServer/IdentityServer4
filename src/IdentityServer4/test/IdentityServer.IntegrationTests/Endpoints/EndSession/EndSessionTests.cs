// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;
using IdentityModel;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityServer4.IntegrationTests.Endpoints.EndSession
{
    public class EndSessionTests
    {
        private const string Category = "End session endpoint";

        private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();
        private Client _wsfedClient;

        public EndSessionTests()
        {
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client1",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client1/callback" },
                FrontChannelLogoutUri = "https://client1/signout",
                PostLogoutRedirectUris = new List<string> { "https://client1/signout-callback" },
                AllowAccessTokensViaBrowser = true
            });

            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client2",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client2/callback" },
                FrontChannelLogoutUri = "https://client2/signout",
                PostLogoutRedirectUris = new List<string> {
                    "https://client2/signout-callback",
                    "https://client2/signout-callback2"
                },
                AllowAccessTokensViaBrowser = true
            });

            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "client3",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client3/callback" },
                BackChannelLogoutUri = "https://client3/signout",
                AllowAccessTokensViaBrowser = true
            });

            _mockPipeline.Clients.Add(_wsfedClient = new Client
            {
                ClientId = "client4",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "openid" },
                RedirectUris = new List<string> { "https://client4/callback" },
                FrontChannelLogoutUri = "https://client4/signout",
                AllowAccessTokensViaBrowser = true
            });

            _mockPipeline.Users.Add(new TestUser
            {
                SubjectId = "bob",
                Username = "bob",
                Claims = new Claim[]
                {
                    new Claim("name", "Bob Loblaw"),
                    new Claim("email", "bob@loblaw.com"),
                    new Claim("role", "Attorney")
                }
            });

            _mockPipeline.IdentityScopes.AddRange(new IdentityResource[] {
                new IdentityResources.OpenId()
            });

            _mockPipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _mockPipeline.BackChannelClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_request_should_redirect_to_logout_page()
        {
            var response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_redirect_to_configured_logout_path()
        {
            _mockPipeline.Options.UserInteraction.LogoutUrl = "/logout";
            _mockPipeline.Options.UserInteraction.LogoutIdParameter = "id";

            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token +
                "&post_logout_redirect_uri=https://client1/signout-callback");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://server/logout?id=");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task logout_request_with_params_should_pass_values_in_logout_context()
        {
            await _mockPipeline.LoginAsync("bob");

            var authorization = await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");

            var id_token = authorization.IdentityToken;

            var response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token +
                "&post_logout_redirect_uri=https://client2/signout-callback2");

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
            _mockPipeline.LogoutRequest.Should().NotBeNull();
            _mockPipeline.LogoutRequest.ClientId.Should().Be("client2");
            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().Be("https://client2/signout-callback2");

            var parts = _mockPipeline.LogoutRequest.SignOutIFrameUrl.Split('?');
            parts[0].Should().Be(IdentityServerPipeline.EndSessionCallbackEndpoint);
            var iframeUrl = QueryHelpers.ParseNullableQuery(parts[1]);
            iframeUrl["endSessionId"].FirstOrDefault().Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task logout_request_with_params_but_user_no_longer_authenticated_should_pass_redirect_info_to_logout()
        {
            await _mockPipeline.LoginAsync("bob");

            var authorization = await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");

            var id_token = authorization.IdentityToken;

            _mockPipeline.RemoveLoginCookie();

            var response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token +
                "&post_logout_redirect_uri=https://client2/signout-callback2");

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
            _mockPipeline.LogoutRequest.Should().NotBeNull();
            _mockPipeline.LogoutRequest.ClientId.Should().Be("client2");
            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().Be("https://client2/signout-callback2");
            _mockPipeline.LogoutRequest.SignOutIFrameUrl.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_should_support_POST()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("id_token_hint", id_token));
            values.Add(new KeyValuePair<string, string>("post_logout_redirect_uri", "https://client1/signout-callback"));
            var content = new FormUrlEncodedContent(values);
            response = await _mockPipeline.BrowserClient.PostAsync(IdentityServerPipeline.EndSessionEndpoint, content);

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
            _mockPipeline.LogoutRequest.Should().NotBeNull();
            _mockPipeline.LogoutRequest.ClientId.Should().Be("client1");
            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().Be("https://client1/signout-callback");

            var parts = _mockPipeline.LogoutRequest.SignOutIFrameUrl.Split('?');
            parts[0].Should().Be(IdentityServerPipeline.EndSessionCallbackEndpoint);
            var iframeUrl = QueryHelpers.ParseNullableQuery(parts[1]);
            iframeUrl["endSessionId"].FirstOrDefault().Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_callback_without_params_should_return_400()
        {
            var response = await _mockPipeline.BackChannelClient.GetAsync(IdentityServerPipeline.EndSessionCallbackEndpoint);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_callback_with_mismatched_post_logout_redirect_uri_should_not_pass_along_logout_message()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token +
                "&post_logout_redirect_uri=https://client1/signout-callback-not-valid");

            var signoutFrameUrl = _mockPipeline.LogoutRequest.SignOutIFrameUrl;

            response = await _mockPipeline.BrowserClient.GetAsync(signoutFrameUrl);

            _mockPipeline.LogoutRequest.ClientId.Should().BeNull();
            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_callback_with_mismatched_id_token_hint_should_not_pass_along_logout_message()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            await _mockPipeline.LoginAsync("alice");

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token +
                "&post_logout_redirect_uri=https://client1/signout-callback");

            _mockPipeline.LogoutRequest.ClientId.Should().BeNull();
            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_signout_callback_should_return_200_html()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            var signoutFrameUrl = _mockPipeline.LogoutRequest.SignOutIFrameUrl;

            response = await _mockPipeline.BrowserClient.GetAsync(signoutFrameUrl);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/html");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_signout_callback_should_render_iframes_for_all_clients()
        {
            await _mockPipeline.LoginAsync("bob");
            var sid = _mockPipeline.GetSessionCookie().Value;

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            var url2 = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response2 = await _mockPipeline.BrowserClient.GetAsync(url2);

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            var signoutFrameUrl = _mockPipeline.LogoutRequest.SignOutIFrameUrl;

            response = await _mockPipeline.BrowserClient.GetAsync(signoutFrameUrl);
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("https://client1/signout?sid=" + sid + "&iss=" + UrlEncoder.Default.Encode("https://server"));
            html.Should().Contain("https://client2/signout?sid=" + sid + "&iss=" + UrlEncoder.Default.Encode("https://server"));
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signout_callback_should_use_signoutcleanup_for_wsfed_client()
        {
            await _mockPipeline.LoginAsync("bob");
            var sid = _mockPipeline.GetSessionCookie().Value;

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client4",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client4/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            var signoutFrameUrl = _mockPipeline.LogoutRequest.SignOutIFrameUrl;

            // since we don't have real ws-fed, we used OIDC to signin, but fooling this
            // at signout to use ws-fed so we can test the iframe params
            _wsfedClient.ProtocolType = ProtocolTypes.WsFederation;

            response = await _mockPipeline.BrowserClient.GetAsync(signoutFrameUrl);
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("https://client4/signout?wa=wsignoutcleanup1.0");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_id_token_hint_but_no_post_logout_redirect_uri_should_use_single_registered_post_logout_redirect_uri()
        {
            await _mockPipeline.LoginAsync("bob");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint + 
                "?id_token_hint=" + id_token);

            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().Be("https://client1/signout-callback");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_id_token_hint_but_no_post_logout_redirect_uri_should_not_use_any_of_multiple_registered_post_logout_redirect_uri()
        {
            await _mockPipeline.LoginAsync("bob");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            var id_token = authorization.IdentityToken;

            _mockPipeline.BrowserClient.AllowAutoRedirect = true;
            response = await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint +
                "?id_token_hint=" + id_token);

            _mockPipeline.LogoutRequest.PostLogoutRedirectUri.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task logout_with_clients_should_render_signout_callback_iframe()
        {
            await _mockPipeline.LoginAsync("bob");

            var response = await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");
            response.Should().NotBeNull();

            await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
            _mockPipeline.LogoutRequest.SignOutIFrameUrl.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task logout_without_clients_should_not_render_signout_callback_iframe()
        {
            await _mockPipeline.LoginAsync("bob");

            await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            _mockPipeline.LogoutWasCalled.Should().BeTrue();
            _mockPipeline.LogoutRequest.SignOutIFrameUrl.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task logout_should_invoke_back_channel_logout()
        {
            _mockPipeline.BackChannelMessageHandler.OnInvoke = async req =>
            {
                req.RequestUri.ToString().Should().StartWith("https://client3/signout");

                var form = await req.Content.ReadAsStringAsync();
                form.Should().Contain(OidcConstants.BackChannelLogoutRequest.LogoutToken);

                var token = form.Split('=')[1];
                var parts = token.Split('.');
                parts.Length.Should().Be(3);

                var bytes = Base64Url.Decode(parts[1]);
                var json = Encoding.UTF8.GetString(bytes);
                var payload = JObject.Parse(json);
                payload["iss"].ToString().Should().Be("https://server");
                payload["sub"].ToString().Should().Be("bob");
                payload["aud"].ToString().Should().Be("client3");
                payload["iat"].Should().NotBeNull();
                payload["jti"].Should().NotBeNull();
                payload["sid"].Should().NotBeNull();
                payload["events"].Type.Should().Be(JTokenType.Object);

                var events = (JObject)payload["events"];
                events.Count.Should().Be(1);
                events["http://schemas.openid.net/event/backchannel-logout"].Should().NotBeNull();
                events["http://schemas.openid.net/event/backchannel-logout"].Type.Should().Be(JTokenType.Object);

                var evt = (JObject)events["http://schemas.openid.net/event/backchannel-logout"];
                evt.Count.Should().Be(0);
            };

            await _mockPipeline.LoginAsync("bob");

            var response = await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client3",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client3/callback",
                state: "123_state",
                nonce: "123_nonce");
            response.Should().NotBeNull();

            await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            var signoutFrameUrl = _mockPipeline.LogoutRequest.SignOutIFrameUrl;
            await _mockPipeline.BrowserClient.GetAsync(signoutFrameUrl);

            _mockPipeline.BackChannelMessageHandler.InvokeWasCalled.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task back_channel_logout_should_not_affect_end_session_callback()
        {
            _mockPipeline.BackChannelMessageHandler.OnInvoke = req => throw new Exception("boom!");

            await _mockPipeline.LoginAsync("bob");

            var response = await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId: "client3",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client3/callback",
                state: "123_state",
                nonce: "123_nonce");
            response.Should().NotBeNull();

            await _mockPipeline.BrowserClient.GetAsync(IdentityServerPipeline.EndSessionEndpoint);

            var signoutFrameUrl = _mockPipeline.LogoutRequest.SignOutIFrameUrl;
            await _mockPipeline.BrowserClient.GetAsync(signoutFrameUrl);

            _mockPipeline.BackChannelMessageHandler.InvokeWasCalled.Should().BeTrue();
        }
    }
}
