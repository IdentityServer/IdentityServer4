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
    class StubResultGenerator : IAuthorizeEndpointResultGenerator
    {
        public IEndpointResult AuthorizeResult { get; set; } = new AuthorizeRedirectResult(null, new FakeUrlEncoder());
        public IEndpointResult ConsentResult { get; set; } = new ConsentPageResult();
        public IEndpointResult LoginResult { get; set; } = new LoginPageResult("http://server/login?id=x");
        public IEndpointResult ErrorResult { get; set; } = new ErrorPageResult(null);

        public Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response)
        {
            return Task.FromResult(AuthorizeResult);
        }

        public Task<IEndpointResult> CreateConsentResultAsync()
        {
            return Task.FromResult(ConsentResult);
        }

        public Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(ErrorResult);
        }

        public Task<IEndpointResult> CreateLoginResultAsync(SignInMessage message)
        {
            return Task.FromResult(LoginResult);
        }
    }
}
