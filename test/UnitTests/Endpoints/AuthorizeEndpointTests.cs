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

namespace IdentityServer4.Tests.Endpoints
{
    public class AuthorizeEndpointTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpoint _subject;

        public AuthorizeEndpointTests()
        {
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = _httpContext;
            var ctx = new IdentityServerContext(accessor, _options);

            _subject = new AuthorizeEndpoint(
                new FakeEventService(), 
                new FakeLogger<AuthorizeEndpoint>(), 
                ctx,
                new StubAuthorizeRequestValidator(_requestValidationResult));
        }

        IdentityServerOptions _options = new IdentityServerOptions();
        DefaultHttpContext _httpContext = new DefaultHttpContext();
        AuthorizeRequestValidationResult _requestValidationResult = new AuthorizeRequestValidationResult();

        [Fact]
        [Trait("Category", Category)]
        public async Task POST_returns_405()
        {
            _httpContext.Request.Method = "POST";
            var result = await _subject.ProcessAsync(_httpContext);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }
    }
}