// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Endpoints.Results
{
    public class AuthorizationResultFactoryTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpointResultFactory _subject;

        ILogger<AuthorizeEndpointResultFactory> _fakeLogger = TestLogger.Create<AuthorizeEndpointResultFactory>();
        IdentityServerOptions _options = TestIdentityServerOptions.Create();
        MockHttpContextAccessor _context;
        MockClientListCookie _mockClientListCookie;
        StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();
        MockMessageStore<IdentityServer4.Models.ErrorMessage> _mockErrorMessageStore = new MockMessageStore<IdentityServer4.Models.ErrorMessage>();

        public AuthorizationResultFactoryTests()
        {
            _context = new MockHttpContextAccessor(_options);
            _mockClientListCookie = new MockClientListCookie(_options, _context);
            _stubAuthorizeResponseGenerator.Response.Request = _validatedRequest;

            _subject = new AuthorizeEndpointResultFactory(
                _fakeLogger,
                _options,
                _context,
                _stubAuthorizeResponseGenerator,
                _mockErrorMessageStore, 
                _mockClientListCookie);
        }
        
        ValidatedAuthorizeRequest _validatedRequest = new ValidatedAuthorizeRequest
        {
            ResponseMode = "fragment",
            ClientId = "client_id",
            Subject = IdentityServerPrincipal.Create("bob", "Bob Loblaw"),
            Client = new Client
            {
                ClientId = "client_id",
                ClientName = "Test Client"
            },
            Raw = new NameValueCollection()
            {
                {"foo","bar"}
            }
        };

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_should_return_error_page()
        {
            var result = await _subject.CreateErrorResultAsync(_validatedRequest, "error");

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_model_should_have_correct_data()
        {
            _context.HttpContext.TraceIdentifier = "555";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(_validatedRequest, "error"));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.Error.Should().Be("error");
            model.Data.RequestId.Should().Be("555");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_with_unknown_response_mode_query_should_throw()
        {
            _validatedRequest.ResponseMode = "unknown";

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _subject.CreateErrorResultAsync(_validatedRequest, "access_denied"));
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_access_denied_error_should_return_authorize_result()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.PromptMode = "none";

            var result = await _subject.CreateErrorResultAsync(_validatedRequest, "access_denied");

            (result is AuthorizeRedirectResult || result is AuthorizeFormPostResult).Should().BeTrue();
        }

        [Theory]
        [InlineData("access_denied")]
        [InlineData("account_selection_required")]
        [InlineData("login_required")]
        [InlineData("interaction_required")]
        [InlineData("consent_required")]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_prompt_mode_none_for_valid_errors_should_return_authorize_result(string error)
        {
            _validatedRequest.PromptMode = "none";

            var result = await _subject.CreateErrorResultAsync(_validatedRequest, error);

            (result is AuthorizeRedirectResult || result is AuthorizeFormPostResult).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_prompt_mode_none_for_invalid_error_should_return_error_page()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.PromptMode = "none";

            var result = await _subject.CreateErrorResultAsync(_validatedRequest, "foo");

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_should_return_authorize_result()
        {
            var result = await _subject.CreateAuthorizeResultAsync(_validatedRequest);

            (result is AuthorizeRedirectResult || result is AuthorizeFormPostResult).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_should_track_clientid_for_successful_results()
        {
            await _subject.CreateAuthorizeResultAsync(_validatedRequest);

            _mockClientListCookie.Clients.Should().Contain("client_id");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_should_not_track_clientid_for_error_results()
        {
            _stubAuthorizeResponseGenerator.Response.IsError = true;

            await _subject.CreateAuthorizeResultAsync(_validatedRequest);

            _mockClientListCookie.Clients.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_with_response_mode_fragment_should_return_redirect()
        {
            _validatedRequest.ResponseMode = "fragment";

            var result = await _subject.CreateAuthorizeResultAsync(_validatedRequest);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_with_response_mode_query_should_return_redirect()
        {
            _validatedRequest.ResponseMode = "query";

            var result = await _subject.CreateAuthorizeResultAsync(_validatedRequest);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_with_response_mode_post_should_return_post()
        {
            _validatedRequest.ResponseMode = "form_post";

            var result = await _subject.CreateAuthorizeResultAsync(_validatedRequest);

            result.Should().BeOfType<AuthorizeFormPostResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateLoginResultAsync_should_return_login_result()
        {
            var result = await _subject.CreateLoginResultAsync(_validatedRequest);
            result.Should().BeAssignableTo<LoginPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateLoginResultAsync_should_generate_returnurl_to_authorize_after_login_with_raw_params()
        {
            var result = (LoginPageResult)(await _subject.CreateLoginResultAsync(_validatedRequest));
            result.ParamValue.Should().Be("/connect/authorize/login?foo=bar");
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task CreateConsentResultAsync_should_return_consent_result()
        {
            var result = await _subject.CreateConsentResultAsync(_validatedRequest);
            result.Should().BeAssignableTo<ConsentPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateConsentResultAsync_should_generate_returnurl_to_authorize_after_consent_with_raw_params()
        {
            var result = (ConsentPageResult)(await _subject.CreateConsentResultAsync(_validatedRequest));
            result.ParamValue.Should().Be("/connect/authorize/consent?foo=bar");
        }
    }
}
