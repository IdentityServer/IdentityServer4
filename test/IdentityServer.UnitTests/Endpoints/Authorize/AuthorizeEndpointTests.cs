// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Endpoints;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Specialized;
using System.Security.Claims;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using IdentityServer4.Models;
using IdentityServer4.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Extensions;
using IdentityServer4.UnitTests.Common;

namespace IdentityServer4.UnitTests.Endpoints.Authorize
{
    public class AuthorizeEndpointTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpoint _subject;

        NameValueCollection _params = new NameValueCollection();
        ClaimsPrincipal _user = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

        HttpContext _context;
        ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        TestEventService _fakeEventService = new TestEventService();
        ILogger<AuthorizeEndpoint> _fakeLogger = TestLogger.Create<AuthorizeEndpoint>();
        StubAuthorizeRequestValidator _stubAuthorizeRequestValidator = new StubAuthorizeRequestValidator();
        StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator = new StubAuthorizeInteractionResponseGenerator();
        MockMessageStore<ConsentResponse> _mockUserConsentResponseMessageStore = new MockMessageStore<ConsentResponse>();
        StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator = new StubAuthorizeResponseGenerator();

        public AuthorizeEndpointTests()
        {
            Init();
        }

        public void Init()
        {
            _context = new MockHttpContextAccessor().HttpContext;

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
            _stubAuthorizeResponseGenerator.Response.Request = _validatedAuthorizeRequest;

            _stubAuthorizeRequestValidator.Result.IsError = false;
            _stubAuthorizeRequestValidator.Result.ValidatedRequest = _validatedAuthorizeRequest;

            _subject = new AuthorizeEndpoint(
                _fakeEventService,
                _fakeLogger,
                _stubAuthorizeRequestValidator,
                _stubInteractionGenerator,
                _mockUserConsentResponseMessageStore,
                _stubAuthorizeResponseGenerator);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_post_to_entry_point_should_return_405()
        {
            _context.Request.Method = "POST";

            var result = await _subject.ProcessAsync(_context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_invalid_path_should_return_404()
        {
            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/foo");

            var result = await _subject.ProcessAsync(_context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(404);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_path_should_return_authorization_result()
        {
            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize");
            _context.SetUser(_user);

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_after_login_path_should_return_authorization_result()
        {
            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/login");
            _context.SetUser(_user);

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
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
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(new ConsentResponse()));

            _context.SetUser(_user);

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/consent");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_request_validation_produces_error_should_display_error_page()
        {
            _stubAuthorizeRequestValidator.Result.IsError = true;
            _stubAuthorizeRequestValidator.Result.Error = "some_error";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task interaction_produces_error_should_show_error_page()
        {
            _stubInteractionGenerator.Response.Error = "error";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
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

            result.Should().BeOfType<AuthorizeResult>();
        }

        // after login
        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeAfterLoginAsync_no_user_should_return_error_page()
        {
            _context.SetUser(null);

            var result = await _subject.ProcessAuthorizeAfterLoginAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }


        // after consent
        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_no_user_should_return_error_page()
        {
            _context.SetUser(null);

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_no_consent_message_should_return_error_page()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseMessageStore.Messages.Add(request.Id, null);

            _context.SetUser(_user);

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/consent");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_consent_missing_consent_data_should_return_error_page()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(null));

            _context.SetUser(_user);

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/consent");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_valid_consent_message_should_return_authorize_result()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(new ConsentResponse() {ScopesConsented = new string[] { "api1", "api2" } }));

            _context.SetUser(_user);

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/consent");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeWithConsentAsync_valid_consent_message_should_cleanup_consent_cookie()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(new ConsentResponse() { ScopesConsented = new string[] { "api1", "api2" } }));

            _context.SetUser(_user);

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/consent");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAuthorizeAfterConsentAsync(_context);

            _mockUserConsentResponseMessageStore.Messages.Count.Should().Be(0);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ProcessAuthorizeRequestAsync_custom_interaction_redirect_result_should_issue_redirect()
        {
            _context.SetUser(_user);
            _stubInteractionGenerator.Response.RedirectUrl = "http://foo.com";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<CustomRedirectResult>();
        }
    }
}
