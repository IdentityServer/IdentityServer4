using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;

namespace IdentityServer4.Core.ResponseHandling
{
    public interface ITokenResponseGenerator
    {
        Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request);
    }
}