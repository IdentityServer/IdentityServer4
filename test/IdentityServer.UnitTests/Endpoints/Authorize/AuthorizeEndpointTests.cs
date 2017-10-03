// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdentityServer4.UnitTests.Endpoints.Authorize
{
    public class AuthorizeEndpointTests
    {
        private const string Category = "Authorize Endpoint";

        private HttpContext _context;

        private TestEventService _fakeEventService = new TestEventService();

        private ILogger<AuthorizeEndpoint> _fakeLogger = TestLogger.Create<AuthorizeEndpoint>();

        private MockUserSession _mockUserSession = new MockUserSession();

        private NameValueCollection _params = new NameValueCollection();

        private StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();

        private StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();

        private StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();

        private AuthorizeEndpoint _subject;

        private ClaimsPrincipal _user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

        private ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        public AuthorizeEndpointTests()
        {
            this.Init();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_path_should_return_authorization_result()
        {
            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize");
            this._mockUserSession.User = this._user;

            var result = await this._subject.ProcessAsync(this._context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_post_without_form_content_type_should_return_415()
        {
            this._context.Request.Method = "POST";

            var result = await this._subject.ProcessAsync(this._context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(415);
        }

        internal void Init()
        {
            this._context = new MockHttpContextAccessor().HttpContext;

            this._validatedAuthorizeRequest = new ValidatedAuthorizeRequest()
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
                Raw = this._params,
                Subject = this._user
            };
            this._stubAuthorizeResponseGenerator.Response.Request = this._validatedAuthorizeRequest;

            this._stubAuthorizeRequestValidator.Result = new AuthorizeRequestValidationResult(this._validatedAuthorizeRequest);

            this._subject = new AuthorizeEndpoint(
                this._fakeEventService,
                this._fakeLogger,
                this._stubAuthorizeRequestValidator,
                this._stubInteractionGenerator,
                this._stubAuthorizeResponseGenerator,
                this._mockUserSession);
        }
    }
}
