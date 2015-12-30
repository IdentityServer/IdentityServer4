using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    interface IAuthorizeEndpointResultGenerator
    {
        Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateLoginResultAsync(ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest validatedRequest);
        Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response);
    }
}
