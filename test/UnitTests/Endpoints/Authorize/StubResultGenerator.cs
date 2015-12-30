using IdentityServer4.Core.ResponseHandling;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using UnitTests.Common;
using System.Collections.Specialized;
using IdentityServer4.Core.Hosting;

namespace UnitTests.Endpoints.Authorize
{
    class StubResultGenerator : IAuthorizeEndpointResultGenerator
    {
        public IEndpointResult AuthorizeResult { get; set; } = new AuthorizeRedirectResult(null);
        public IEndpointResult ConsentResult { get; set; } = new ConsentPageResult("consent_id");
        public IEndpointResult LoginResult { get; set; } = new LoginPageResult("login_id");
        public IEndpointResult ErrorResult { get; set; } = new ErrorPageResult(null);

        public Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response)
        {
            return Task.FromResult(AuthorizeResult);
        }

        public Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest validatedRequest, NameValueCollection parameters)
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
