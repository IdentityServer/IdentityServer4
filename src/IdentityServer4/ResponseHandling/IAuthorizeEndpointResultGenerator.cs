using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    interface IAuthorizeEndpointResultGenerator
    {
        Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateLoginResultAsync(SignInMessage message);
        Task<IEndpointResult> CreateConsentResultAsync();
        Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response);
    }
}
