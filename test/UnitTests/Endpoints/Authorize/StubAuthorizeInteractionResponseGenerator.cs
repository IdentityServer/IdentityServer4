using IdentityServer4.Core.ResponseHandling;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;
using System.Security.Claims;

namespace UnitTests.Endpoints.Authorize
{
    class StubAuthorizeInteractionResponseGenerator : IAuthorizeInteractionResponseGenerator
    {
        internal LoginInteractionResponse LoginResponse { get; set; } = new LoginInteractionResponse();
        internal ConsentInteractionResponse ConsentResponse { get; set; } = new ConsentInteractionResponse();

        public Task<LoginInteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            return Task.FromResult(LoginResponse);
        }

        public Task<ConsentInteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, UserConsent consent = null)
        {
            return Task.FromResult(ConsentResponse);
        }
    }
}
