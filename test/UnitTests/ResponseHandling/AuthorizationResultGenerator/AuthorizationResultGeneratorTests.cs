using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.ResponseHandling;
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

            _clientListCookie = new ClientListCookie(_context);

            _subject = new AuthorizationResultGenerator(
                _fakeLogger,
                _context,
                _stubLocalizationService,
                new FakeHtmlEncoder(),
                _clientListCookie);
        }

        ILogger<AuthorizationResultGenerator> _fakeLogger = new FakeLogger<AuthorizationResultGenerator>();
        IdentityServerOptions _options = new IdentityServerOptions();
        DefaultHttpContext _httpContext = new DefaultHttpContext();
        IdentityServerContext _context;
        ClientListCookie _clientListCookie;

        StubLocalizationService _stubLocalizationService = new StubLocalizationService();

        //[Fact]
        //[Trait("Category", Category)]
        //public async Task authorize_request_validation_failure_with_user_error_should_display_error_page_with_error_view_model()
        //{
        //    _stubAuthorizeRequestValidator.Result.IsError = true;
        //    _stubAuthorizeRequestValidator.Result.ErrorType = ErrorTypes.User;
        //    _stubAuthorizeRequestValidator.Result.Error = "foo";
        //    //_stubLocalizationService.Result = "foo error message";
        //    _context.SetRequestId("56789");

        //    var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        //    result.Should().BeOfType<ErrorPageResult>();
        //    var error_result = (ErrorPageResult)result;
        //    error_result.Model.RequestId.Should().Be("56789");
        //    error_result.Model.ErrorCode.Should().Be("foo");
        //    error_result.Model.ErrorMessage.Should().Be("foo error message");
        //}

        //[Fact]
        //[Trait("Category", Category)]
        //public async Task authorize_request_validation_failure_error_page_should_contain_return_info()
        //{
        //    _stubAuthorizeRequestValidator.Result.IsError = true;
        //    _stubAuthorizeRequestValidator.Result.ErrorType = ErrorTypes.Client;
        //    _stubAuthorizeRequestValidator.Result.ValidatedRequest.RedirectUri = "http://client/callback";
        //    _stubAuthorizeRequestValidator.Result.ValidatedRequest.State = "123";
        //    _stubAuthorizeRequestValidator.Result.ValidatedRequest.ResponseMode = "fragment";
        //    _stubAuthorizeRequestValidator.Result.ValidatedRequest.ClientId = "foo_client";
        //    _stubAuthorizeRequestValidator.Result.ValidatedRequest.Client = new Client
        //    {
        //        ClientId = "foo_client",
        //        ClientName = "Foo Client"
        //    };

        //    var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

        //    var error_result = (ErrorPageResult)result;
        //    error_result.Model.ReturnInfo.Should().NotBeNull();
        //    error_result.Model.ReturnInfo.ClientId.Should().Be("foo_client");
        //    error_result.Model.ReturnInfo.ClientName.Should().Be("Foo Client");
        //    var parts = error_result.Model.ReturnInfo.Uri.Split('#');
        //    parts.Length.Should().Be(2);
        //    parts[0].Should().Be("http://client/callback");
        //    parts[1].Should().Contain("state=123");
        //}

        /*
[Fact]
        [Trait("Category", Category)]
        public async Task response_mode_fragment_should_generate_authorize_redirect_result()
        {
            _validatedAuthorizeRequest.ResponseMode = "fragment";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task response_mode_query_should_generate_authorize_redirect_result()
        {
            _validatedAuthorizeRequest.ResponseMode = "query";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<AuthorizeRedirectResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task response_mode_formpost_should_generate_authorize_redirect_result()
        {
            _validatedAuthorizeRequest.ResponseMode = "form_post";

            var result = await _subject.ProcessAuthorizeRequestAsync(_params, _user, null);

            result.Should().BeOfType<AuthorizeFormPostResult>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task unknown_response_mode_on_validated_authorize_request_should_throw()
        {
            _validatedAuthorizeRequest.ResponseMode = "foo";
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _subject.ProcessAuthorizeRequestAsync(_params, _user, null));
        }
        */
    }
}
