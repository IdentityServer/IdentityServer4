// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.UnitTests.Common;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultUserSessionTests
    {
        DefaultUserSession _subject;
        MockHttpContextAccessor _mockHttpContext = new MockHttpContextAccessor();
        IdentityServerOptions _options = new IdentityServerOptions();
        ClaimsPrincipal _user;
        AuthenticationProperties _props = new AuthenticationProperties();

        public DefaultUserSessionTests()
        {
            _user = IdentityServerPrincipal.Create("123", "bob");
            _subject = new DefaultUserSession(_mockHttpContext, _options, TestLogger.Create<DefaultUserSession>());
        }

        [Fact]
        public void CreateSessionId_when_user_is_anonymous_should_generate_new_sid()
        {
            _subject.CreateSessionId(_user, _props);

            _props.Items[DefaultUserSession.SessionIdKey].Should().NotBeNull();
        }

        [Fact]
        public void CreateSessionId_when_user_is_authenticated_should_not_generate_new_sid()
        {
            // this test is needed to allow same session id when cookie is slid
            // IOW, if UI layer passes in same properties dictionary, then we assume it's the same user

            _props.Items.Add(DefaultUserSession.SessionIdKey, "999");
            _subject.SetCurrentUser(_user, _props);

            _subject.CreateSessionId(_user, _props);

            _props.Items[DefaultUserSession.SessionIdKey].Should().NotBeNull();
            _props.Items[DefaultUserSession.SessionIdKey].Should().Be("999");
        }

        [Fact]
        public void CreateSessionId_when_user_is_authenticated_but_different_sub_should_create_new_sid()
        {
            _props.Items.Add(DefaultUserSession.SessionIdKey, "999");
            _subject.SetCurrentUser(_user, _props);

            _subject.CreateSessionId(IdentityServerPrincipal.Create("alice", "alice"), _props);

            _props.Items[DefaultUserSession.SessionIdKey].Should().NotBeNull();
            _props.Items[DefaultUserSession.SessionIdKey].Should().NotBe("999");
        }

        [Fact]
        public void CreateSessionId_should_issue_session_id_cookie()
        {
            _subject.CreateSessionId(_user, _props);

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName).FirstOrDefault();
            cookie.Value.Should().Be(_props.Items[DefaultUserSession.SessionIdKey]);
        }

        [Fact]
        public void EnsureSessionIdCookieAsync_should_add_cookie()
        {
            _props.Items.Add(DefaultUserSession.SessionIdKey, "999");
            _subject.SetCurrentUser(_user, _props);

            _subject.EnsureSessionIdCookie();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName).FirstOrDefault();
            cookie.Value.Should().Be("999");
        }

        [Fact]
        public void EnsureSessionIdCookieAsync_should_not_add_cookie_if_no_sid()
        {
            _subject.EnsureSessionIdCookie();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName).FirstOrDefault();
            cookie.Should().BeNull();
        }

        [Fact]
        public void RemoveSessionIdCookie_should_remove_cookie()
        {
            _props.Items.Add(DefaultUserSession.SessionIdKey, "999");
            _subject.SetCurrentUser(_user, _props);

            _subject.EnsureSessionIdCookie();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            string cookie = cookieContainer.GetCookieHeader(new Uri("http://server"));
            _mockHttpContext.HttpContext.Request.Headers.Add("Cookie", cookie);

            _subject.RemoveSessionIdCookie();

            cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));

            var query = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName);
            query.Count().Should().Be(0);
        }

        [Fact]
        public void GetCurrentSessionIdAsync_when_user_is_authenticated_should_return_sid()
        {
            _props.Items.Add(DefaultUserSession.SessionIdKey, "999");
            _subject.SetCurrentUser(_user, _props);

            var sid = _subject.SessionId;
            sid.Should().Be("999");
        }

        [Fact]
        public void GetCurrentSessionIdAsync_when_user_is_anonymous_should_return_null()
        {
            var sid = _subject.SessionId;
            sid.Should().BeNull();
        }

        [Fact]
        public async Task adding_client_should_set_item_in_cookie_properties()
        {
            _subject.SetCurrentUser(_user, _props);

            _props.Items.Count.Should().Be(0);
            await _subject.AddClientIdAsync("client");
            _props.Items.Count.Should().Be(1);
        }

        [Fact]
        public void when_authenticated_GetIdentityServerUserAsync_should_should_return_authenticated_user()
        {
            _subject.SetCurrentUser(_user, _props);

            var user = _subject.User;
            user.GetSubjectId().Should().Be("123");
        }

        [Fact]
        public void when_anonymous_GetIdentityServerUserAsync_should_should_return_null()
        {
            var user = _subject.User;
            user.Should().BeNull();
        }

        [Fact]
        public async Task corrupt_properties_entry_should_clear_entry()
        {
            _subject.SetCurrentUser(_user, _props);

            await _subject.AddClientIdAsync("client");
            var item = _props.Items.First();
            _props.Items[item.Key] = "junk";

            var clients = await _subject.GetClientListAsync();
            clients.Should().BeEmpty();
            _props.Items.Count.Should().Be(0);
        }

        [Fact]
        public async Task adding_client_should_be_able_to_read_client()
        {
            _subject.SetCurrentUser(_user, _props);

            await _subject.AddClientIdAsync("client");
            var clients = await _subject.GetClientListAsync();
            clients.Should().Contain(new string[] { "client" });
        }

        [Fact]
        public async Task adding_clients_should_be_able_to_read_clients()
        {
            _subject.SetCurrentUser(_user, _props);

            await _subject.AddClientIdAsync("client1");
            await _subject.AddClientIdAsync("client2");
            var clients = await _subject.GetClientListAsync();
            clients.Should().Contain(new string[] { "client2", "client1" });
        }
    }
}
