// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.UnitTests.Common;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultClientSessionServiceTests
    {
        DefaultClientSessionService _subject;
        MockHttpContextAccessor _mockHttpContext = new MockHttpContextAccessor();
        MockSessionIdService _stubSessionId = new MockSessionIdService();
        StubAuthenticationHandler _stubAuthHandler;
        IdentityServerOptions _options = new IdentityServerOptions();
        ClaimsPrincipal _user;

        public DefaultClientSessionServiceTests()
        {
            _user = new ClaimsPrincipal(new ClaimsIdentity("password"));
            _stubAuthHandler = new StubAuthenticationHandler(_user, IdentityServerConstants.DefaultCookieAuthenticationScheme);
            _mockHttpContext.HttpContext.GetAuthentication().Handler = _stubAuthHandler;

            _subject = new DefaultClientSessionService(_mockHttpContext, _stubSessionId, _options, TestLogger.Create<DefaultClientSessionService>());
        }

        [Fact]
        public async Task adding_client_should_set_item_in_cookie_properties()
        {
            _stubAuthHandler.Properties.Count.Should().Be(0);
            await _subject.AddClientIdAsync("client");
            _stubAuthHandler.Properties.Count.Should().Be(1);
        }

        [Fact]
        public async Task corrupt_properties_entry_should_clear_entry()
        {
            await _subject.AddClientIdAsync("client");
            var item = _stubAuthHandler.Properties.First();
            _stubAuthHandler.Properties[item.Key] = "junk";

            var clients = await _subject.GetClientListAsync();
            clients.Should().BeEmpty();
            _stubAuthHandler.Properties.Count.Should().Be(0);
        }

        [Fact]
        public async Task adding_client_should_be_able_to_read_client()
        {
            await _subject.AddClientIdAsync("client");
            var clients = await _subject.GetClientListAsync();
            clients.Should().Contain(new string[] { "client" });
        }

        [Fact]
        public async Task adding_clients_should_be_able_to_read_clients()
        {
            await _subject.AddClientIdAsync("client1");
            await _subject.AddClientIdAsync("client2");
            var clients = await _subject.GetClientListAsync();
            clients.Should().Contain(new string[] { "client2", "client1" });
        }

        [Fact]
        public async Task EnsureClientListCookieAsync_should_add_cookie()
        {
            await _subject.AddClientIdAsync("client");
            await _subject.EnsureClientListCookieAsync(await _stubSessionId.GetCurrentSessionIdAsync());
            _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Count().Should().Be(1);
        }

        [Fact]
        public async Task EnsureClientListCookieAsync_should_not_add_cookie_if_no_clients_in_list()
        {
            await _subject.EnsureClientListCookieAsync(await _stubSessionId.GetCurrentSessionIdAsync());
            _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Count().Should().Be(0);
        }

        [Fact]
        public async Task GetClientListFromCookie_should_get_list_from_cookie()
        {
            await _subject.AddClientIdAsync("client1");
            await _subject.AddClientIdAsync("client2");
            await _subject.EnsureClientListCookieAsync(await _stubSessionId.GetCurrentSessionIdAsync());

            var cookie = _mockHttpContext.HttpContext.Response.Headers.First(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Value;
            _mockHttpContext.HttpContext.Request.Headers.Add("Cookie", cookie);

            var clients = _subject.GetClientListFromCookie(await _stubSessionId.GetCurrentSessionIdAsync());
            clients.Should().Contain(new string[] { "client2", "client1" });
        }

        [Fact]
        public async Task RemoveCookie_should_remove_cookie()
        {
            await _subject.AddClientIdAsync("client1");
            await _subject.AddClientIdAsync("client2");
            await _subject.EnsureClientListCookieAsync(await _stubSessionId.GetCurrentSessionIdAsync());

            var cookieContainer = new CookieContainer();
            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x=>x.Value);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            _mockHttpContext.HttpContext.Response.Headers.Clear();

            string cookie = cookieContainer.GetCookieHeader(new Uri("http://server"));
            _mockHttpContext.HttpContext.Request.Headers.Add("Cookie", cookie);

            _subject.RemoveCookie(await _stubSessionId.GetCurrentSessionIdAsync());

            cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookies.Count().Should().BeGreaterThan(0);
            cookieContainer.SetCookies(new Uri("http://server"), string.Join(",", cookies));
            cookieContainer.GetCookies(new Uri("http://server")).Count.Should().Be(0);
        }

        [Fact]
        public async Task RemoveCookie_should_not_remove_cookie_if_no_client_list()
        {
            _subject.RemoveCookie(await _stubSessionId.GetCurrentSessionIdAsync());

            var cookies = _mockHttpContext.HttpContext.Response.Headers.Where(x => x.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
            cookies.Count().Should().Be(0);
        }
    }
}
