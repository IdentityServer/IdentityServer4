// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.Endpoints;
using System.Threading.Tasks;
using Xunit;
using UnitTests.Common;
using IdentityServer4.Hosting;
using System.Collections.Specialized;
using System.Security.Claims;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4;
using IdentityServer4.Endpoints.Results;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Endpoints.Authorize
{
    public class AuthorizeEndpointTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpoint _subject;

        NameValueCollection _params = new NameValueCollection();
        ClaimsPrincipal _user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

        IdentityServerContext _context;
        ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        TestEventService _fakeEventService = new TestEventService();
        ILogger<AuthorizeEndpoint> _fakeLogger = TestLogger.Create<AuthorizeEndpoint>();
        StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();
        StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();
        StubResultFactory _stubResultFactory = new StubResultFactory();
        MockMessageStore<SignInResponse> _mockSignInResponseStore = new MockMessageStore<SignInResponse>();
        MockMessageStore<ConsentResponse> _mockUserConsentResponseMessageStore = new MockMessageStore<ConsentResponse>();

        public AuthorizeEndpointTests()
        {
            Init();
        }

        public void Init()
        {
            _context = IdentityServerContextHelper.Create();

            _validatedAuthorizeRequest = new ValidatedAuthorizeRequest()
            {
                RedirectUri = "http://client/callback",
                State = "123",
                ResponseMode = "fragment",
                ClientId = "client",
                Client = new Client
                {
                    ClientId = "client",
                    ClientName = "Test Client"
                },
                Raw = _params,
                Subject = _user
            };

            _stubAuthorizeRequestValidator.Result.IsError = false;
            _stubAuthorizeRequestValidator.Result.ValidatedRequest = _validatedAuthorizeRequest;

            _subject = new AuthorizeEndpoint(
                _fakeEventService,
                _fakeLogger,
                _context,
                _stubAuthorizeRequestValidator,
                _stubInteractionGenerator,
                _stubResultFactory,
                _mockSignInResponseStore,
                _mockUserConsentResponseMessageStore);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_post_to_entry_point_should_return_405()
        {
            _context.HttpContext.Request.Method = "POST";

            var result = await _subject.ProcessAsync(_context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_invalid_path_should_return_404()
        {
            _context.HttpContext.Request.Method = "GET";
            _context.HttpContext.Request.Path = new PathString("/foo");

            var result = await _subject.ProcessAsync(_context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(404);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_path_should_return_authorization_result()
        {
            _context.HttpContext.Request.Method = "GET";
            _context.HttpContext.Request.Path = new PathString("/connect/authorize");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_with_login_path_without_login_params_should_return_error_page()
        {
            _context.HttpContext.Request.Method = "GET";
            _context.HttpContext.Request.Path = new PathString("/connect/authorize/login");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_with_login_path_should_return_authorization_result()
        {
            _context.HttpContext.Request.Method = "GET";
            _mockSignInResponseStore.Messages.Add("123", new Message<SignInResponse>(new SignInResponse()) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);
            _context.HttpContext.Request.Path = new PathString("/connect/authorize/login");

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_with_consent_path_without_consent_params_should_return_error_page()
        {
            _context.HttpContext.Request.Method = "GET";
            _context.HttpContext.Request.Path = new PathString("/connect/authorize/consent");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_with_consent_path_should_return_authorization_result()
        {
            _context.HttpContext.Request.Method = "GET";
            _mockUserConsentResponseMessageStore.Messages.Add("123", new Message<ConsentResponse>(new ConsentResponse()) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);
            _context.HttpContext.Request.Path = new PathString("/connect/authorize/consent");

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }



        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_produces_error_should_display_error_page()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_produces_error_should_raise_failed_endpoint_event()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;
            _stubAuthorizeRequestValidator.Result.ErrorType = ErrorTypes.Client;
            _stubAuthorizeRequestValidator.Result.Error = "some error";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            var evt = _fakeEventService.AssertEventWasRaised<Event<EndpointDetail>>();
            evt.EventType.Should().Be(EventTypes.Failure);
            evt.Id.Should().Be(EventConstants.Ids.EndpointFailure);
            evt.Message.Should().Be("some error");
            evt.Details.EndpointName.Should().Be(EventConstants.EndpointNames.Authorize);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_produces_error_should_show_error_page()
        {
            _stubInteractionGenerator.Response.Error = new AuthorizeError { };

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_produces_error_should_raise_failed_endpoint_event()
        {
            _stubInteractionGenerator.Response.Error = new AuthorizeError {
                Error = "some_error",
            };

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            var evt = _fakeEventService.AssertEventWasRaised<Event<EndpointDetail>>();
            evt.EventType.Should().Be(EventTypes.Failure);
            evt.Id.Should().Be(EventConstants.Ids.EndpointFailure);
            evt.Message.Should().Be("some_error");
            evt.Details.EndpointName.Should().Be(EventConstants.EndpointNames.Authorize);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_produces_login_result_should_trigger_login()
        {
            _stubInteractionGenerator.Response.IsLogin = true;

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<LoginPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_generator_consent_produces_consent_should_show_consent_page()
        {
            _stubInteractionGenerator.Response.IsConsent = true;

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<ConsentPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task successful_authorization_request_should_generate_authorize_result()
        {
            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        // after login
        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeAfterLoginAsync_no_id_param_should_return_error_page()
        {
            _mockSignInResponseStore.Messages.Add("123", new Message<SignInResponse>(new SignInResponse()));
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterLoginAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeAfterLoginAsync_no_signin_message_should_return_error_page()
        {
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterLoginAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeAfterLoginAsync_signin_missing_params_data_should_return_error_page()
        {
            _mockSignInResponseStore.Messages.Add("123", new Message<SignInResponse>(new SignInResponse()));
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterLoginAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeAfterLoginAsync_valid_signin_message_should_return_authorize_result()
        {
            _mockSignInResponseStore.Messages.Add("123", new Message<SignInResponse>(new SignInResponse()) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterLoginAsync(_context);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeAfterLoginAsync_valid_signin_message_should_cleanup_signin_message_cookie()
        {
            _mockSignInResponseStore.Messages.Add("123", new Message<SignInResponse>(new SignInResponse()) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            await _subject.ProcessAuthorizeAfterLoginAsync(_context);

            _mockSignInResponseStore.Messages.Count.Should().Be(0);
        }

        // after consent

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_no_id_param_should_return_error_page()
        {
            _mockUserConsentResponseMessageStore.Messages.Add("123", new Message<ConsentResponse>(new ConsentResponse()));
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_no_consent_message_should_return_error_page()
        {
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_consent_missing_params_data_should_return_error_page()
        {
            _mockUserConsentResponseMessageStore.Messages.Add("123", new Message<ConsentResponse>(new ConsentResponse()));
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_consent_missing_consent_data_should_return_error_page()
        {
            _mockUserConsentResponseMessageStore.Messages.Add("123", new Message<ConsentResponse>(null) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeAssignableTo<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_valid_consent_message_should_return_authorize_result()
        {
            _mockUserConsentResponseMessageStore.Messages.Add("123", new Message<ConsentResponse>(new ConsentResponse()) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_valid_consent_message_should_cleanup_consent_cookie()
        {
            _mockUserConsentResponseMessageStore.Messages.Add("123", new Message<ConsentResponse>(new ConsentResponse()) { AuthorizeRequestParameters = new Dictionary<string, string>() });
            _context.HttpContext.Request.QueryString = _context.HttpContext.Request.QueryString.Add("id", "123");
            _context.HttpContext.SetUser(_user);

            await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            _mockUserConsentResponseMessageStore.Messages.Count.Should().Be(0);
        }
    }
}
