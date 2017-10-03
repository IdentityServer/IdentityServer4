// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdentityServer4.UnitTests.Endpoints.Authorize
{
    public class AuthorizeEndpointBaseTests
    {
        private const string Category = "Authorize Endpoint";

        private HttpContext _context;

        private TestEventService _fakeEventService = new TestEventService();

        private ILogger<TestAuthorizeEndpoint> _fakeLogger = TestLogger.Create<TestAuthorizeEndpoint>();

        private MockUserSession _mockUserSession = new MockUserSession();

        private NameValueCollection _params = new NameValueCollection();

        private StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();

        private StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();

        private StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();

        private TestAuthorizeEndpoint _subject;

        private ClaimsPrincipal _user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

        private ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        public AuthorizeEndpointBaseTests()
        {
            this.Init();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_produces_error_should_display_error_page()
        {
            this._stubAuthorizeRequestValidator.Result.IsError = true;
            this._stubAuthorizeRequestValidator.Result.Error = "some_error";

            var result = await this._subject.ProcessAuthorizeRequestAsync(this._params, this._user, null);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_generator_consent_produces_consent_should_show_consent_page()
        {
            this._stubInteractionGenerator.Response.IsConsent = true;

            var result = await this._subject.ProcessAuthorizeRequestAsync(this._params, this._user, null);

            result.Should().BeOfType<ConsentPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_produces_error_should_show_error_page()
        {
            this._stubInteractionGenerator.Response.Error = "error";

            var result = await this._subject.ProcessAuthorizeRequestAsync(this._params, this._user, null);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_produces_login_result_should_trigger_login()
        {
            this._stubInteractionGenerator.Response.IsLogin = true;

            var result = await this._subject.ProcessAuthorizeRequestAsync(this._params, this._user, null);

            result.Should().BeOfType<LoginPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeRequestAsync_custom_interaction_redirect_result_should_issue_redirect()
        {
            this._mockUserSession.User = this._user;
            this._stubInteractionGenerator.Response.RedirectUrl = "http://foo.com";

            var result = await this._subject.ProcessAuthorizeRequestAsync(this._params, this._user, null);

            result.Should().BeOfType<CustomRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task successful_authorization_request_should_generate_authorize_result()
        {
            var result = await this._subject.ProcessAuthorizeRequestAsync(this._params, this._user, null);

            result.Should().BeOfType<AuthorizeResult>();
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

            this._subject = new TestAuthorizeEndpoint(
                this._fakeEventService,
                this._fakeLogger,
                this._stubAuthorizeRequestValidator,
                this._stubInteractionGenerator,
                this._stubAuthorizeResponseGenerator,
                this._mockUserSession);
        }

        internal class TestAuthorizeEndpoint : AuthorizeEndpointBase
        {
            public TestAuthorizeEndpoint(
              IEventService events,
              ILogger<TestAuthorizeEndpoint> logger,
              IAuthorizeRequestValidator validator,
              IAuthorizeInteractionResponseGenerator interactionGenerator,
              IAuthorizeResponseGenerator authorizeResponseGenerator,
              IUserSession userSession)
            : base(events, logger, validator, interactionGenerator, authorizeResponseGenerator, userSession)
            {
            }

            public override Task<IEndpointResult> ProcessAsync(HttpContext context)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
