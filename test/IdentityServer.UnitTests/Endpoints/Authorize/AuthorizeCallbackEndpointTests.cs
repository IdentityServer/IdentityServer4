// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdentityServer4.UnitTests.Endpoints.Authorize
{
    public class AuthorizeCallbackEndpointTests
    {
        private const string Category = "Authorize Endpoint";

        private HttpContext _context;

        private TestEventService _fakeEventService = new TestEventService();

        private ILogger<AuthorizeCallbackEndpoint> _fakeLogger = TestLogger.Create<AuthorizeCallbackEndpoint>();

        private MockConsentMessageStore _mockUserConsentResponseMessageStore = new MockConsentMessageStore();

        private MockUserSession _mockUserSession = new MockUserSession();

        private NameValueCollection _params = new NameValueCollection();

        private StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();

        private StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();

        private StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();

        private AuthorizeCallbackEndpoint _subject;

        private ClaimsPrincipal _user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

        private ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        public AuthorizeCallbackEndpointTests()
        {
            this.Init();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_after_consent_path_should_return_authorization_result()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, this._user.GetSubjectId());
            this._mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(new ConsentResponse()));

            this._mockUserSession.User = this._user;

            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize/callback");
            this._context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await this._subject.ProcessAsync(this._context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_after_login_path_should_return_authorization_result()
        {
            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize/callback");
            this._mockUserSession.User = this._user;

            var result = await this._subject.ProcessAsync(this._context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_consent_missing_consent_data_should_return_error_page()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, this._user.GetSubjectId());
            this._mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(null));

            this._mockUserSession.User = this._user;

            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize/callback");
            this._context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await this._subject.ProcessAsync(this._context);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_no_consent_message_should_return_redirect_for_consent()
        {
            this._stubInteractionGenerator.Response.IsConsent = true;

            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, this._user.GetSubjectId());
            this._mockUserConsentResponseMessageStore.Messages.Add(request.Id, null);

            this._mockUserSession.User = this._user;

            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize/callback");
            this._context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await this._subject.ProcessAsync(this._context);

            result.Should().BeOfType<ConsentPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_post_to_entry_point_should_return_405()
        {
            this._context.Request.Method = "POST";

            var result = await this._subject.ProcessAsync(this._context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_valid_consent_message_should_cleanup_consent_cookie()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, this._user.GetSubjectId());
            this._mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(new ConsentResponse() { ScopesConsented = new string[] { "api1", "api2" } }));

            this._mockUserSession.User = this._user;

            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize/callback");
            this._context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await this._subject.ProcessAsync(this._context);

            this._mockUserConsentResponseMessageStore.Messages.Count.Should().Be(0);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_valid_consent_message_should_return_authorize_result()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, this._user.GetSubjectId());
            this._mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(new ConsentResponse() { ScopesConsented = new string[] { "api1", "api2" } }));

            this._mockUserSession.User = this._user;

            this._context.Request.Method = "GET";
            this._context.Request.Path = new PathString("/connect/authorize/callback");
            this._context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await this._subject.ProcessAsync(this._context);

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

            this._subject = new AuthorizeCallbackEndpoint(
                this._fakeEventService,
                this._fakeLogger,
                this._stubAuthorizeRequestValidator,
                this._stubInteractionGenerator,
                this._stubAuthorizeResponseGenerator,
                this._mockUserSession,
                this._mockUserConsentResponseMessageStore);
        }
    }
}
