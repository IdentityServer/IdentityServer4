// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.UnitTests.Common;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultSessionIdServiceTests
    {
        DefaultSessionIdService _subject;
        MockHttpContextAccessor _mockHttpContext = new MockHttpContextAccessor();
        StubAuthenticationHandler _stubAuthHandler;
        IdentityServerOptions _options = new IdentityServerOptions();
        ClaimsPrincipal _user;
        string _scheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;

        public DefaultSessionIdServiceTests()
        {
            _user = IdentityServerPrincipal.Create("123", "bob");
            _stubAuthHandler = new StubAuthenticationHandler(null, _scheme);
            _mockHttpContext.HttpContext.GetAuthentication().Handler = _stubAuthHandler;

            _subject = new DefaultSessionIdService(_mockHttpContext, _options);
        }

        [Fact]
        public void CreateSessionId_when_user_is_anonymous_should_generate_new_sid()
        {
            var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("123", "bob"), null);
            _subject.CreateSessionId(ctx);
            ctx.Properties["sid"].Should().NotBeNull();
        }

        [Fact]
        public void CreateSessionId_when_user_is_authenticated_should_generate_new_sid()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add("sid", "999");

            var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("123", "bob"), null);
            _subject.CreateSessionId(ctx);
            ctx.Properties["sid"].Should().NotBeNull();
            ctx.Properties["sid"].Should().NotBe("999");
        }

        [Fact]
        public void CreateSessionId_when_user_is_authenticated_but_different_sub_should_create_new_sid()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add("sid", "999");

            var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("456", "alice"), null);
            _subject.CreateSessionId(ctx);
            ctx.Properties["sid"].Should().NotBeNull();
            ctx.Properties["sid"].Should().NotBe("999");
        }

        [Fact]
        public void CreateSessionId_should_issue_session_id_cookie()
        {
            var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("456", "alice"), null);
            _subject.CreateSessionId(ctx);

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _subject.GetCookieName()).FirstOrDefault();
            cookie.Value.Should().Be(ctx.Properties["sid"]);
        }

        [Fact]
        public async Task GetCurrentSessionIdAsync_when_user_is_authenticated_should_return_sid()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add("sid", "999");

            var sid = await _subject.GetCurrentSessionIdAsync();
            sid.Should().Be("999");
        }

        [Fact]
        public async Task GetCurrentSessionIdAsync_when_user_is_anonymous_should_return_null()
        {
            var sid = await _subject.GetCurrentSessionIdAsync();
            sid.Should().BeNull();
        }

        [Fact]
        public async Task EnsureSessionCookieAsync_should_add_cookie()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add("sid", "999");

            await _subject.EnsureSessionCookieAsync();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _subject.GetCookieName()).FirstOrDefault();
            cookie.Value.Should().Be("999");
        }

        [Fact]
        public async Task EnsureSessionCookieAsync_should_not_add_cookie_if_no_sid()
        {
            await _subject.EnsureSessionCookieAsync();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _subject.GetCookieName()).FirstOrDefault();
            cookie.Should().BeNull();
        }

        [Fact]
        public async Task GetCookieValue_should_return_value_from_sid_cookie()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add("sid", "999");

            await _subject.EnsureSessionCookieAsync();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            string cookie = cookieContainer.GetCookieHeader(new Uri("http://server"));
            _mockHttpContext.HttpContext.Request.Headers.Add("Cookie", cookie);

            _subject.GetCookieValue().Should().Be("999");
        }

        [Fact]
        public void GetCookieValue_should_return_null_if_no_sid_cookie()
        {
            _subject.GetCookieValue().Should().BeNull();
        }

        [Fact]
        public async Task RemoveCookie_should_remove_cookie()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add("sid", "999");

            await _subject.EnsureSessionCookieAsync();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            string cookie = cookieContainer.GetCookieHeader(new Uri("http://server"));
            _mockHttpContext.HttpContext.Request.Headers.Add("Cookie", cookie);

            _subject.RemoveCookie();

            cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));

            var query = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _subject.GetCookieName());
            query.Count().Should().Be(0);
        }
    }
}
