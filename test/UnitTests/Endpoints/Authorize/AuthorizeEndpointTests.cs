/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FluentAssertions;
using IdentityServer4.Core.Endpoints;
using System.Threading.Tasks;
using Xunit;
using UnitTests.Common;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http.Internal;
using IdentityServer4.Core.Configuration;
using System;
using System.Collections.Specialized;
using System.Security.Claims;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Events;
using IdentityServer4.Core.Models;
using IdentityServer4.Core;

namespace UnitTests.Endpoints.Authorize
{
    public class AuthorizeEndpointTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpoint _subject;

        public AuthorizeEndpointTests()
        {
            Init();
        }

        public void Init()
        {
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = _httpContext;
            _context = new IdentityServerContext(accessor, _options);

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
                Raw = _params
            };

            _stubAuthorizeRequestValidator.Result.IsError = false;
            _stubAuthorizeRequestValidator.Result.ValidatedRequest = _validatedAuthorizeRequest;
            _stubResponseGenerator.Response.Request = _validatedAuthorizeRequest;

            _clientListCookie = new ClientListCookie(_context);

            _subject = new AuthorizeEndpoint(
                _mockEventService, 
                _fakeLogger, 
                _context,
                _stubResponseGenerator,
                _stubAuthorizeRequestValidator,
                _stubInteractionGenerator,
                _stubLocalizationService,
                new FakeHtmlEncoder(),
                _clientListCookie);
        }

        NameValueCollection _params = new NameValueCollection();
        ClaimsPrincipal _user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");
        IdentityServerContext _context;
        IdentityServerOptions _options = new IdentityServerOptions();
        DefaultHttpContext _httpContext = new DefaultHttpContext();
        ClientListCookie _clientListCookie;
        ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        MockEventService _mockEventService = new MockEventService();
        ILogger<AuthorizeEndpoint> _fakeLogger = new FakeLogger<AuthorizeEndpoint>();
        StubLocalizationService _stubLocalizationService = new StubLocalizationService();
        StubAuthorizeResponseGenerator _stubResponseGenerator = new StubAuthorizeResponseGenerator();
        StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();
        StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();

        [Fact]
        [Trait("Category", Category)]
        public async Task post_to_entry_point_should_return_405()
        {
            _httpContext.Request.Method = "POST";
            var result = await _subject.ProcessAsync(_httpContext);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_with_user_error_should_display_error_page_with_error_view_model()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;
            _stubAuthorizeRequestValidator.Result.ErrorType = ErrorTypes.User;
            _stubAuthorizeRequestValidator.Result.Error = "foo";
            _stubLocalizationService.Result = "foo error message";
            _context.SetRequestId("56789");

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<ErrorPageResult>();
            var error_result = (ErrorPageResult)result;
            error_result.Model.RequestId.Should().Be("56789");
            error_result.Model.ErrorCode.Should().Be("foo");
            error_result.Model.ErrorMessage.Should().Be("foo error message");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_with_client_error_should_display_error_page()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<ErrorPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_error_page_should_contain_return_info()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;
            _stubAuthorizeRequestValidator.Result.ErrorType = ErrorTypes.Client;
            _stubAuthorizeRequestValidator.Result.ValidatedRequest.RedirectUri = "http://client/callback";
            _stubAuthorizeRequestValidator.Result.ValidatedRequest.State = "123";
            _stubAuthorizeRequestValidator.Result.ValidatedRequest.ResponseMode = "fragment";
            _stubAuthorizeRequestValidator.Result.ValidatedRequest.ClientId = "foo_client";
            _stubAuthorizeRequestValidator.Result.ValidatedRequest.Client = new Client
            {
                ClientId = "foo_client",
                ClientName = "Foo Client"
            };

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            var error_result = (ErrorPageResult)result;
            error_result.Model.ReturnInfo.Should().NotBeNull();
            error_result.Model.ReturnInfo.ClientId.Should().Be("foo_client");
            error_result.Model.ReturnInfo.ClientName.Should().Be("Foo Client");
            var parts = error_result.Model.ReturnInfo.Uri.Split('#');
            parts.Length.Should().Be(2);
            parts[0].Should().Be("http://client/callback");
            parts[1].Should().Contain("state=123");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_should_raise_failed_endpoint_event()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;
            _stubAuthorizeRequestValidator.Result.ErrorType = ErrorTypes.Client;
            _stubAuthorizeRequestValidator.Result.Error = "some error";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            var evt = _mockEventService.AssertEventWasRaised<Event<EndpointDetail>>();
            evt.EventType.Should().Be(EventTypes.Failure);
            evt.Id.Should().Be(EventConstants.Ids.EndpointFailure);
            evt.Message.Should().Be("some error");
            evt.Details.EndpointName.Should().Be(EventConstants.EndpointNames.Authorize);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task login_interaction_produces_error_should_show_error_page()
        {
            _stubInteractionGenerator.LoginResponse.Error = new AuthorizeError
            {
                ErrorType = ErrorTypes.User,
                Error = "some error",
            };

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<ErrorPageResult>();
            var error_result = (ErrorPageResult)result;
            error_result.Model.ReturnInfo.Should().BeNull();
            error_result.Model.ErrorCode.Should().Be("some error");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task login_interaction_produces_login_result_should_trigger_login()
        {
            var msg = new SignInMessage { };
            _stubInteractionGenerator.LoginResponse.SignInMessage = msg;

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<LoginRedirectResult>();
            var redirect = (LoginRedirectResult)result;
            redirect.SignInMessage.Should().BeSameAs(msg);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task client_login_interaction_produces_login_result_should_trigger_login()
        {
            var msg = new SignInMessage { };
            _stubInteractionGenerator.ClientLoginResponse.SignInMessage = msg;

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<LoginRedirectResult>();
            var redirect = (LoginRedirectResult)result;
            redirect.SignInMessage.Should().BeSameAs(msg);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task successful_authorization_request_should_generate_authorization_result()
        {
            _stubResponseGenerator.Response.IdentityToken = "foo";
            _stubResponseGenerator.Response.AccessToken = "bar";
            _stubResponseGenerator.Response.State = "789";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeAssignableTo<AuthorizeResult>();
            var authorize_result = (AuthorizeResult)result;
            authorize_result.Response.IdentityToken.Should().Be("foo");
            authorize_result.Response.AccessToken.Should().Be("bar");
            authorize_result.Response.State.Should().Be("789");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task response_mode_fragment_should_generate_authorization_redirect_result()
        {
            _validatedAuthorizeRequest.ResponseMode = "fragment";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task response_mode_query_should_generate_authorization_redirect_result()
        {
            _validatedAuthorizeRequest.ResponseMode = "query";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task response_mode_formpost_should_generate_authorization_redirect_result()
        {
            _validatedAuthorizeRequest.ResponseMode = "form_post";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user);

            result.Should().BeOfType<AuthorizeFormPostResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task unknown_response_mode_should_throw()
        {
            _validatedAuthorizeRequest.ResponseMode = "foo";
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _subject.ProcessAuthorizeRequestAsync(_params, _user));
        }
    }
}