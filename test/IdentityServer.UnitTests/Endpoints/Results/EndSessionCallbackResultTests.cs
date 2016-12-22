// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Endpoints.Results
{
    public class EndSessionCallbackResultTests
    {
        EndSessionCallbackResult _subject;

        EndSessionCallbackValidationResult _result = new EndSessionCallbackValidationResult();
        MockSessionIdService _mockSessionId = new MockSessionIdService();
        MockClientSessionService _mockClientSession = new MockClientSessionService();
        MockMessageStore<LogoutMessage> _mockLogoutMessageStore = new MockMessageStore<LogoutMessage>();
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        DefaultHttpContext _context = new DefaultHttpContext();

        public EndSessionCallbackResultTests()
        {
            _context.SetIdentityServerOrigin("https://server");
            _context.SetIdentityServerBasePath("/");
            _context.Response.Body = new MemoryStream();

            _subject = new EndSessionCallbackResult(_result, _mockClientSession, _mockLogoutMessageStore, _options);
        }

        [Fact]
        public async Task logout_message_should_be_removed()
        {
            _mockLogoutMessageStore.Messages.Add("1", new Message<LogoutMessage>(new LogoutMessage()));
            _result.LogoutId = "1";

            await _subject.ExecuteAsync(_context);

            _mockLogoutMessageStore.Messages.Count.Should().Be(0);
        }

        [Fact]
        public async Task error_should_return_400()
        {
            _result.IsError = true;

            await _subject.ExecuteAsync(_context);

            _context.Response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task success_should_clear_cookies()
        {
            _result.IsError = false;
            _result.SessionId = "5";

            await _subject.ExecuteAsync(_context);

            _mockClientSession.RemoveCookieWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task success_should_render_html_and_iframes()
        {
            _result.IsError = false;
            _result.ClientLogoutUrls = new string[] { "http://foo.com", "http://bar.com" };

            await _subject.ExecuteAsync(_context);

            _context.Response.ContentType.Should().StartWith("text/html");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var rdr = new StreamReader(_context.Response.Body))
            {
                var html = rdr.ReadToEnd();
                html.Should().Contain("<iframe src='http://foo.com'></iframe>");
                html.Should().Contain("<iframe src='http://bar.com'></iframe>");
            }
        }
    }
}
