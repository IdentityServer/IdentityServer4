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
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultUserSessionTests
    {
        DefaultUserSession _subject;
        MockHttpContextAccessor _mockHttpContext = new MockHttpContextAccessor();
        StubAuthenticationHandler _stubAuthHandler;
        IdentityServerOptions _options = new IdentityServerOptions();
        ClaimsPrincipal _user;
        string _scheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;

        public DefaultUserSessionTests()
        {
            _user = IdentityServerPrincipal.Create("123", "bob");
            _stubAuthHandler = new StubAuthenticationHandler(null, IdentityServerConstants.DefaultCookieAuthenticationScheme);
            _mockHttpContext.HttpContext.GetAuthentication().Handler = _stubAuthHandler;

            _subject = new DefaultUserSession(_mockHttpContext, _options, TestLogger.Create<DefaultUserSession>());
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public void CreateSessionId_when_user_is_anonymous_should_generate_new_sid()
        {
            //var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("123", "bob"), null);
            //_subject.CreateSessionId(ctx);
            //ctx.Properties[DefaultUserSession.SessionIdKey].Should().NotBeNull();
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public void CreateSessionId_when_user_is_authenticated_should_generate_new_sid()
        {
            //_stubAuthHandler.User = _user;
            //_stubAuthHandler.Properties.Add("sid", "999");

            //var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("123", "bob"), null);
            //_subject.CreateSessionId(ctx);
            //ctx.Properties[DefaultUserSession.SessionIdKey].Should().NotBeNull();
            //ctx.Properties[DefaultUserSession.SessionIdKey].Should().NotBe("999");
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public void CreateSessionId_when_user_is_authenticated_but_different_sub_should_create_new_sid()
        {
            //_stubAuthHandler.User = _user;
            //_stubAuthHandler.Properties.Add("sid", "999");

            //var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("456", "alice"), null);
            //_subject.CreateSessionId(ctx);
            //ctx.Properties[DefaultUserSession.SessionIdKey].Should().NotBeNull();
            //ctx.Properties[DefaultUserSession.SessionIdKey].Should().NotBe("999");
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public void CreateSessionId_should_issue_session_id_cookie()
        {
            //var ctx = new SignInContext(_scheme, IdentityServerPrincipal.Create("456", "alice"), null);
            //_subject.CreateSessionId(ctx);

            //var cookieContainer = new CookieContainer();
            //var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            //cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            //_mockHttpContext.HttpContext.Response.Headers.Clear();

            //var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName).FirstOrDefault();
            //cookie.Value.Should().Be(ctx.Properties[DefaultUserSession.SessionIdKey]);
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task EnsureSessionIdCookieAsync_should_add_cookie()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add(DefaultUserSession.SessionIdKey, "999");

            await _subject.EnsureSessionIdCookieAsync();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName).FirstOrDefault();
            cookie.Value.Should().Be("999");
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task EnsureSessionIdCookieAsync_should_not_add_cookie_if_no_sid()
        {
            await _subject.EnsureSessionIdCookieAsync();

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            var cookie = cookieContainer.GetCookies(new Uri("http://server")).Cast<Cookie>().Where(x => x.Name == _options.Authentication.CheckSessionCookieName).FirstOrDefault();
            cookie.Should().BeNull();
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task RemoveSessionIdCookie_should_remove_cookie()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add(DefaultUserSession.SessionIdKey, "999");

            await _subject.EnsureSessionIdCookieAsync();

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

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task GetCurrentSessionIdAsync_when_user_is_authenticated_should_return_sid()
        {
            _stubAuthHandler.User = _user;
            _stubAuthHandler.Properties.Add(DefaultUserSession.SessionIdKey, "999");

            var sid = await _subject.GetCurrentSessionIdAsync();
            sid.Should().Be("999");
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task GetCurrentSessionIdAsync_when_user_is_anonymous_should_return_null()
        {
            var sid = await _subject.GetCurrentSessionIdAsync();
            sid.Should().BeNull();
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task adding_client_should_set_item_in_cookie_properties()
        {
            _stubAuthHandler.User = _user;

            _stubAuthHandler.Properties.Count.Should().Be(0);
            await _subject.AddClientIdAsync("client");
            _stubAuthHandler.Properties.Count.Should().Be(1);
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task when_authenticated_GetIdentityServerUserAsync_should_should_return_authenticated_user()
        {
            _stubAuthHandler.User = _user;

            var user = await _subject.GetIdentityServerUserAsync();
            user.GetSubjectId().Should().Be("123");
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task when_anonymous_GetIdentityServerUserAsync_should_should_return_null()
        {
            _stubAuthHandler.User = null;
            var user = await _subject.GetIdentityServerUserAsync();
            user.Should().BeNull();
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task corrupt_properties_entry_should_clear_entry()
        {
            _stubAuthHandler.User = _user;

            await _subject.AddClientIdAsync("client");
            var item = _stubAuthHandler.Properties.First();
            _stubAuthHandler.Properties[item.Key] = "junk";

            var clients = await _subject.GetClientListAsync();
            clients.Should().BeEmpty();
            _stubAuthHandler.Properties.Count.Should().Be(0);
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task adding_client_should_be_able_to_read_client()
        {
            _stubAuthHandler.User = _user;

            await _subject.AddClientIdAsync("client");
            var clients = await _subject.GetClientListAsync();
            clients.Should().Contain(new string[] { "client" });
        }

        [Fact(Skip = "AuthenticationService re-work")]
        public async Task adding_clients_should_be_able_to_read_clients()
        {
            _stubAuthHandler.User = _user;

            await _subject.AddClientIdAsync("client1");
            await _subject.AddClientIdAsync("client2");
            var clients = await _subject.GetClientListAsync();
            clients.Should().Contain(new string[] { "client2", "client1" });
        }
    }
}
