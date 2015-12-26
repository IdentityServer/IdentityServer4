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

namespace IdentityServer4.Tests.Endpoints
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
            var ctx = new IdentityServerContext(accessor, _options);

            _subject = new AuthorizeEndpoint(
                _mockEventService, 
                _fakeLogger, 
                ctx,
                new StubAuthorizeRequestValidator(_requestValidationResult));
        }

        MockEventService _mockEventService = new MockEventService();
        ILogger<AuthorizeEndpoint> _fakeLogger = new FakeLogger<AuthorizeEndpoint>();

        IdentityServerOptions _options = new IdentityServerOptions();
        DefaultHttpContext _httpContext = new DefaultHttpContext();
        AuthorizeRequestValidationResult _requestValidationResult = new AuthorizeRequestValidationResult();

        [Fact]
        [Trait("Category", Category)]
        public async Task post_to_entry_point_returns_405()
        {
            _httpContext.Request.Method = "POST";
            var result = await _subject.ProcessAsync(_httpContext);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_with_user_error_should_display_error_page()
        {
            _requestValidationResult.IsError = true;
            _requestValidationResult.ErrorType = ErrorTypes.User;

            var param = new NameValueCollection();
            var result = await _subject.ProcessRequestAsync(param, null);

            var error_result = result as ErrorPageResult;
            error_result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_with_client_error_should_return_to_client()
        {
            _requestValidationResult.IsError = true;
            _requestValidationResult.ErrorType = ErrorTypes.Client;

            var param = new NameValueCollection();
            var result = await _subject.ProcessRequestAsync(param, null);

            var error_result = result as ClientErrorResult;
            error_result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_failure_raises_failed_endpoint_event()
        {
            _requestValidationResult.IsError = true;
            _requestValidationResult.ErrorType = ErrorTypes.Client;
            _requestValidationResult.Error = "some error";

            var param = new NameValueCollection();
            var result = await _subject.ProcessRequestAsync(param, null);

            var evt = _mockEventService.AssertEventWasRaised<Event<EndpointDetail>>();
            evt.EventType.Should().Be(EventTypes.Failure);
            evt.Id.Should().Be(EventConstants.Ids.EndpointFailure);
            evt.Message.Should().Be("some error");
            evt.Details.EndpointName.Should().Be(EventConstants.EndpointNames.Authorize);
        }
    }
}