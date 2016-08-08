﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Endpoints.Results
{
    public class AuthorizationResultFactoryTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpointResultFactory _subject;

        ILogger<AuthorizeEndpointResultFactory> _fakeLogger = TestLogger.Create<AuthorizeEndpointResultFactory>();
        IdentityServerContext _context = IdentityServerContextHelper.Create();
        MockClientListCookie _mockClientListCookie;
        StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();
        MockMessageStore<IdentityServer4.Models.ErrorMessage> _mockErrorMessageStore = new MockMessageStore<IdentityServer4.Models.ErrorMessage>();
        TestLocalizationService _stubLocalizationService = new TestLocalizationService();

        public AuthorizationResultFactoryTests()
        {
            _mockClientListCookie = new MockClientListCookie(_context);
            _stubAuthorizeResponseGenerator.Response.Request = _validatedRequest;

            _subject = new AuthorizeEndpointResultFactory(
                _fakeLogger,
                _context,
                _stubAuthorizeResponseGenerator,
                _stubLocalizationService,
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
            var result = await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_model_should_have_correct_data()
        {
            _context.HttpContext.TraceIdentifier = "555";
            _stubLocalizationService.Result = "translation";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ErrorCode.Should().Be("error");
            model.Data.ErrorDescription.Should().Be("translation");
            model.Data.RequestId.Should().Be("555");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_should_use_error_code_for_error_message_when_no_localization()
        {
            _stubLocalizationService.Result = null;

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ErrorDescription.Should().Be("error");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_user_error_should_not_have_return_info()
        {
            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ReturnInfo.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_should_have_return_info()
        {
            _validatedRequest.RedirectUri = "http://client/callback";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ReturnInfo.Should().NotBeNull();
            model.Data.ReturnInfo.ClientId.Should().Be("client_id");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_form_post_should_have_correct_url_and_post_body()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "form_post";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ReturnInfo.IsPost.Should().BeTrue();
            model.Data.ReturnInfo.Uri.Should().Be("http://client/callback");
            model.Data.ReturnInfo.PostBody.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_fragment_should_have_correct_url()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "fragment";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ReturnInfo.IsPost.Should().BeFalse();
            model.Data.ReturnInfo.Uri.Should().StartWith("http://client/callback#");
            model.Data.ReturnInfo.Uri.Should().Contain("state=123");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_query_should_have_correct_url()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "query";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.ParamValue];
            model.Data.ReturnInfo.IsPost.Should().BeFalse();
            model.Data.ReturnInfo.Uri.Should().StartWith("http://client/callback?");
            model.Data.ReturnInfo.Uri.Should().Contain("state=123");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_with_unknown_response_mode_query_should_throw()
        {
            _validatedRequest.ResponseMode = "unknown";

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_access_denied_error_should_return_authorize_result()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.PromptMode = "none";

            var result = await _subject.CreateErrorResultAsync(ErrorTypes.Client, "access_denied", _validatedRequest);

            (result is AuthorizeRedirectResult || result is AuthorizeFormPostResult).Should().BeTrue();
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_prompt_mode_none_client_does_not_allow_none_should_return_error_page()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.PromptMode = "none";

            var result = await _subject.CreateErrorResultAsync(ErrorTypes.Client, "login_required", _validatedRequest);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Theory]
        [InlineData("login_required")]
        [InlineData("interaction_required")]
        [InlineData("consent_required")]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_prompt_mode_none_client_allows_none_for_valid_errors_should_return_authorize_result(string error)
        {
            _validatedRequest.PromptMode = "none";
            _validatedRequest.Client.AllowPromptNone = true;

            var result = await _subject.CreateErrorResultAsync(ErrorTypes.Client, error, _validatedRequest);

            (result is AuthorizeRedirectResult || result is AuthorizeFormPostResult).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_prompt_mode_none_client_allows_none_for_invalid_error_should_return_error_page()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.PromptMode = "none";
            _validatedRequest.Client.AllowPromptNone = true;

            var result = await _subject.CreateErrorResultAsync(ErrorTypes.Client, "foo", _validatedRequest);

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
