using FluentAssertions;
using IdentityServer4.Core;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Common;
using Xunit;

namespace UnitTests.ResponseHandling
{
    public class AuthorizationResultGeneratorTests
    {
        const string Category = "Authorize Endpoint";

        AuthorizeEndpointResultGenerator _subject;

        public AuthorizationResultGeneratorTests()
        {
            Init();
        }

        public void Init()
        {
            _mockClientListCookie = new MockClientListCookie(_context);

            _subject = new AuthorizeEndpointResultGenerator(
                _fakeLogger,
                _context,
                _stubLocalizationService,
                _mockSignInMessageStore,
                _mockConsentRequestMessageStore,
                _mockErrorMessageStore, 
                _mockClientListCookie);
        }

        ILogger<AuthorizeEndpointResultGenerator> _fakeLogger = new FakeLogger<AuthorizeEndpointResultGenerator>();
        IdentityServerContext _context = IdentityServerContextHelper.Create();
        MockClientListCookie _mockClientListCookie;
        MockMessageStore<SignInMessage> _mockSignInMessageStore = new MockMessageStore<SignInMessage>();
        MockMessageStore<UserConsentRequestMessage> _mockConsentRequestMessageStore = new MockMessageStore<UserConsentRequestMessage>();
        MockMessageStore<IdentityServer4.Core.Models.ErrorMessage> _mockErrorMessageStore = new MockMessageStore<IdentityServer4.Core.Models.ErrorMessage>();
        StubLocalizationService _stubLocalizationService = new StubLocalizationService();

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
        };


        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_model_should_have_correct_data()
        {
            _context.SetRequestId("555");
            _stubLocalizationService.Result = "translation";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ErrorCode.Should().Be("error");
            model.ErrorDescription.Should().Be("translation");
            model.RequestId.Should().Be("555");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_error_result_should_use_error_code_for_error_message_when_no_localization()
        {
            _stubLocalizationService.Result = null;

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ErrorDescription.Should().Be("error");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_user_error_should_not_have_return_info()
        {
            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.User, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ReturnInfo.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_should_have_return_info()
        {
            _validatedRequest.RedirectUri = "http://client/callback";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ReturnInfo.Should().NotBeNull();
            model.ReturnInfo.ClientId.Should().Be("client_id");
            model.ReturnInfo.ClientName.Should().Be("Test Client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_form_post_should_have_correct_url_and_post_body()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "form_post";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ReturnInfo.IsPost.Should().BeTrue();
            model.ReturnInfo.Uri.Should().Be("http://client/callback");
            model.ReturnInfo.PostBody.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_fragment_should_have_correct_url()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "fragment";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ReturnInfo.IsPost.Should().BeFalse();
            model.ReturnInfo.Uri.Should().StartWith("http://client/callback#");
            model.ReturnInfo.Uri.Should().Contain("state=123");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateErrorResultAsync_client_error_with_response_mode_query_should_have_correct_url()
        {
            _validatedRequest.State = "123";
            _validatedRequest.RedirectUri = "http://client/callback";
            _validatedRequest.ResponseMode = "query";

            var result = (ErrorPageResult)(await _subject.CreateErrorResultAsync(ErrorTypes.Client, "error", _validatedRequest));

            var model = _mockErrorMessageStore.Messages[result.Id];
            model.ReturnInfo.IsPost.Should().BeFalse();
            model.ReturnInfo.Uri.Should().StartWith("http://client/callback?");
            model.ReturnInfo.Uri.Should().Contain("state=123");
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
            var result = await _subject.CreateLoginResultAsync(_validatedRequest);

            result.Should().BeAssignableTo<LoginPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateLoginResultAsync_should_store_signin_message()
        {
            _mockSignInMessageStore.Messages.Count.Should().Be(0);

            await _subject.CreateLoginResultAsync(_validatedRequest);

            _mockSignInMessageStore.Messages.Count.Should().Be(1);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateLoginResultAsync_should_generate_redirect_with_signin_message_id()
        {
            var result = (LoginPageResult)await _subject.CreateLoginResultAsync(_validatedRequest);

            var id = _mockSignInMessageStore.Messages.First().Key;
            result.Id.Should().Be(id);
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task CreateConsentResultAsync_should_return_consent_result()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                Raw = new NameValueCollection()
            };
            var result = await _subject.CreateConsentResultAsync(request);

            result.Should().BeAssignableTo<ConsentPageResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateConsentResultAsync_should_store_consent_request_message()
        {
            _mockConsentRequestMessageStore.Messages.Count.Should().Be(0);

            var request = new ValidatedAuthorizeRequest()
            {
                Raw = new NameValueCollection()
            };
            var result = await _subject.CreateConsentResultAsync(request);

            _mockConsentRequestMessageStore.Messages.Count.Should().Be(1);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task CreateConsentResultAsync_should_generate_redirect_with_consent_message_id()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                Raw = new NameValueCollection()
            };
            var result = (ConsentPageResult)await _subject.CreateConsentResultAsync(request);

            var id = _mockConsentRequestMessageStore.Messages.First().Key;
            result.Id.Should().Be(id);
        }
    }
}
