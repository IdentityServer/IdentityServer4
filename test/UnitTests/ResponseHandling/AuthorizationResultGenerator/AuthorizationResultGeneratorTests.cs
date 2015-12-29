using FluentAssertions;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Common;
using Xunit;

namespace UnitTests.ResponseHandling
{
    public class AuthorizationResultGeneratorTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizationResultGenerator _subject;

        public AuthorizationResultGeneratorTests()
        {
            Init();
        }

        public void Init()
        {
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = _httpContext;
            _context = new IdentityServerContext(accessor, _options);

            _mockClientListCookie = new MockClientListCookie(_context);

            _subject = new AuthorizationResultGenerator(
                _fakeLogger,
                _context,
                _stubLocalizationService,
                new FakeHtmlEncoder(),
                _mockClientListCookie);
        }

        ILogger<AuthorizationResultGenerator> _fakeLogger = new FakeLogger<AuthorizationResultGenerator>();
        IdentityServerOptions _options = new IdentityServerOptions();
        DefaultHttpContext _httpContext = new DefaultHttpContext();
        IdentityServerContext _context;
        MockClientListCookie _mockClientListCookie;

        ValidatedAuthorizeRequest _validatedRequest = new ValidatedAuthorizeRequest
        {
            ResponseMode = "fragment",
            ClientId = "client_id",
            Subject = IdentityServerPrincipal.Create("bob", "Bob Loblaw"),
            Client = new IdentityServer4.Core.Models.Client
            {
                ClientId = "client_id",
                ClientName = "Test Client"
            }
        };

        StubLocalizationService _stubLocalizationService = new StubLocalizationService();

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_model_should_have_correct_data()
        {
            _context.SetRequestId("555");
            _stubLocalizationService.Result = "translation";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            result.Model.ErrorCode.Should().Be("error");
            result.Model.ErrorMessage.Should().Be("translation");
            result.Model.RequestId.Should().Be("555");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_should_use_error_code_for_error_message_when_no_localization()
        {
            _stubLocalizationService.Result = null;

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            result.Model.ErrorMessage.Should().Be("error");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_user_error_should_not_have_return_info()
        {
            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            result.Model.ReturnInfo.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_should_have_return_info()
        {
            _validatedRequest.RedirectUri = "http://client/callback";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            result.Model.ReturnInfo.Should().NotBeNull();
            result.Model.ReturnInfo.ClientId.Should().Be("client_id");
            result.Model.ReturnInfo.ClientName.Should().Be("Test Client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_form_post_should_have_correct_url_and_post_body()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "form_post";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            result.Model.ReturnInfo.IsPost.Should().BeTrue();
            result.Model.ReturnInfo.Uri.Should().Be("http://client/callback");
            result.Model.ReturnInfo.PostBody.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_fragment_should_have_correct_url()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "fragment";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            result.Model.ReturnInfo.IsPost.Should().BeFalse();
            result.Model.ReturnInfo.Uri.Should().StartWith("http://client/callback#");
            result.Model.ReturnInfo.Uri.Should().Contain("state=123");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_query_should_have_correct_url()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "query";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            result.Model.ReturnInfo.IsPost.Should().BeFalse();
            result.Model.ReturnInfo.Uri.Should().StartWith("http://client/callback?");
            result.Model.ReturnInfo.Uri.Should().Contain("state=123");
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
        public async Task CreateErrorResultAsync_client_error_with_prompt_mode_none_should_return_authorize_result()
        {
            _validatedRequest.PromptMode = "none";

            var result = await _subject.CreateErrorResultAsync(ErrorTypes.Client, "foo", _validatedRequest);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_should_return_authorize_result()
        {
            var response = new AuthorizeResponse()
            {
                Request = _validatedRequest
            };

            var result = await _subject.CreateAuthorizeResultAsync(response);

            result.Should().BeAssignableTo<AuthorizeResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_should_track_clientid()
        {
            var response = new AuthorizeResponse()
            {
                Request = _validatedRequest
            };

            await _subject.CreateAuthorizeResultAsync(response);

            _mockClientListCookie.Clients.Should().Contain("client_id");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_with_response_mode_fragment_should_return_redirect()
        {
            _validatedRequest.ResponseMode = "fragment";
            var response = new AuthorizeResponse()
            {
                Request = _validatedRequest
            };

            var result = await _subject.CreateAuthorizeResultAsync(response);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_with_response_mode_query_should_return_redirect()
        {
            _validatedRequest.ResponseMode = "query";
            var response = new AuthorizeResponse()
            {
                Request = _validatedRequest
            };

            var result = await _subject.CreateAuthorizeResultAsync(response);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateAuthorizeResultAsync_with_response_mode_post_should_return_post()
        {
            _validatedRequest.ResponseMode = "form_post";
            var response = new AuthorizeResponse()
            {
                Request = _validatedRequest
            };

            var result = await _subject.CreateAuthorizeResultAsync(response);

            result.Should().BeOfType<AuthorizeFormPostResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateLoginResultAsync_should_return_login_result()
        {
            var result = await _subject.CreateLoginResultAsync(new IdentityServer4.Core.Models.SignInMessage());

            result.Should().BeAssignableTo<LoginPageResult>();
        }
    }
}
