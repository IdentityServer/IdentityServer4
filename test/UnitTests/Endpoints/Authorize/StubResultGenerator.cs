using IdentityServer4.Core.ResponseHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.Hosting;
using UnitTests.Common;

namespace UnitTests.Endpoints.Authorize
{
    class StubResultGenerator : IAuthorizationResultGenerator
    {
        public IResult AuthorizeResult { get; set; } = new AuthorizeRedirectResult(null, new FakeUrlEncoder());
        public IResult ConsentResult { get; set; } = new ConsentPageResult();
        public IResult LoginResult { get; set; } = new LoginPageResult("http://server/login?id=x");
        public IResult ErrorResult { get; set; } = new ErrorPageResult(null);

        public Task<IResult> CreateAuthorizeResultAsync(AuthorizeResponse response)
        {
            return Task.FromResult(AuthorizeResult);
        }

        public Task<IResult> CreateConsentResultAsync()
        {
            return Task.FromResult(ConsentResult);
        }

        public Task<IResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(ErrorResult);
        }

        public Task<IResult> CreateLoginResultAsync(SignInMessage message)
        {
            return Task.FromResult(LoginResult);
        }
    }
}
