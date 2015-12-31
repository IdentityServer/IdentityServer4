using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public interface ITokenValidator
    {
        Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null);
        Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true);
    }
}