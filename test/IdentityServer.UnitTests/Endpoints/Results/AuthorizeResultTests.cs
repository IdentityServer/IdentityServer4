﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Endpoints.Results
{
    public class AuthorizeResultTests
    {
        AuthorizeResult _subject;

        AuthorizeResponse _response = new AuthorizeResponse();
        IdentityServerOptions _options = new IdentityServerOptions();
        MockClientSessionService _mockClientSession = new MockClientSessionService();
        MockMessageStore<Models.ErrorMessage> _mockErrorMessageStore = new MockMessageStore<Models.ErrorMessage>();

        DefaultHttpContext _context = new DefaultHttpContext();

        public AuthorizeResultTests()
        {
            _context.SetOrigin("https://server");
            _context.SetBasePath("/");
            _context.Response.Body = new MemoryStream();

            _options.UserInteractionOptions.ErrorUrl = "~/error";
            _options.UserInteractionOptions.ErrorIdParameter = "errorId";

            _subject = new AuthorizeResult(_response, _options, _mockClientSession, _mockErrorMessageStore);
        }

        [Fact]
        public async Task error_should_redirect_to_error_page_and_passs_info()
        {
            _response.Error = "some_error";

            await _subject.ExecuteAsync(_context);

            _mockErrorMessageStore.Messages.Count.Should().Be(1);
            _context.Response.StatusCode.Should().Be(302);
            var location = _context.Response.Headers["Location"].First();
            location.Should().StartWith("https://server/error");
            var query = QueryHelpers.ParseQuery(new Uri(location).Query);
            query["errorId"].First().Should().Be(_mockErrorMessageStore.Messages.First().Key);
        }

        [Theory]
        [InlineData(OidcConstants.AuthorizeErrors.AccountSelectionRequired)]
        [InlineData(OidcConstants.AuthorizeErrors.LoginRequired)]
        [InlineData(OidcConstants.AuthorizeErrors.ConsentRequired)]
        [InlineData(OidcConstants.AuthorizeErrors.InteractionRequired)]
        public async Task prompt_none_errors_should_return_to_client(string error)
        {
            _response.Error = error;
            _response.Request = new ValidatedAuthorizeRequest
            {
                ResponseMode = OidcConstants.ResponseModes.Query,
                RedirectUri = "http://client/callback",
                PromptMode = "none"
            };

            await _subject.ExecuteAsync(_context);

            _mockClientSession.Clients.Count.Should().Be(0);
            _context.Response.StatusCode.Should().Be(302);
            var location = _context.Response.Headers["Location"].First();
            location.Should().StartWith("http://client/callback");
        }

        [Fact]
        public async Task access_denied_should_return_to_client()
        {
            _response.Error = OidcConstants.AuthorizeErrors.AccessDenied;
            _response.Request = new ValidatedAuthorizeRequest
            {
                ResponseMode = OidcConstants.ResponseModes.Query,
                RedirectUri = "http://client/callback",
            };

            await _subject.ExecuteAsync(_context);

            _mockClientSession.Clients.Count.Should().Be(0);
            _context.Response.StatusCode.Should().Be(302);
            var location = _context.Response.Headers["Location"].First();
            location.Should().StartWith("http://client/callback");
        }

        [Fact]
        public async Task success_should_add_client_to_client_list()
        {
            _response.Request = new ValidatedAuthorizeRequest
            {
                ClientId = "client",
                ResponseMode = OidcConstants.ResponseModes.Query,
                RedirectUri = "http://client/callback",
            };

            await _subject.ExecuteAsync(_context);

            _mockClientSession.Clients.Should().Contain("client");
        }

        [Fact]
        public async Task query_mode_should_pass_results_in_query()
        {
            _response.Request = new ValidatedAuthorizeRequest
            {
                ClientId = "client",
                ResponseMode = OidcConstants.ResponseModes.Query,
                RedirectUri = "http://client/callback",
                State = "state"
            };

            await _subject.ExecuteAsync(_context);

            _context.Response.StatusCode.Should().Be(302);
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
            var location = _context.Response.Headers["Location"].First();
            location.Should().StartWith("http://client/callback");
            location.Should().Contain("?state=state");
        }

        [Fact]
        public async Task fragment_mode_should_pass_results_in_fragment()
        {
            _response.Request = new ValidatedAuthorizeRequest
            {
                ClientId = "client",
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                RedirectUri = "http://client/callback",
                State = "state"
            };

            await _subject.ExecuteAsync(_context);

            _context.Response.StatusCode.Should().Be(302);
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
            var location = _context.Response.Headers["Location"].First();
            location.Should().StartWith("http://client/callback");
            location.Should().Contain("#state=state");
        }

        [Fact]
        public async Task form_post_mode_should_pass_results_in_body()
        {
            _response.Request = new ValidatedAuthorizeRequest
            {
                ClientId = "client",
                ResponseMode = OidcConstants.ResponseModes.FormPost,
                RedirectUri = "http://client/callback",
                State = "state"
            };

            await _subject.ExecuteAsync(_context);

            _context.Response.StatusCode.Should().Be(200);
            _context.Response.ContentType.Should().StartWith("text/html");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
            _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var rdr = new StreamReader(_context.Response.Body))
            {
                var html = rdr.ReadToEnd();
                html.Should().Contain("<form method='post' action='http://client/callback'>");
                html.Should().Contain("<input type='hidden' name='state' value='state' />");
            }
        }
    }
}
