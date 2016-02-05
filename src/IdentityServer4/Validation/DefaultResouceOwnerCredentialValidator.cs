using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public class DefaultResouceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task<CustomGrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request)
        {
            var result = new CustomGrantValidationResult("unsupported_grant_type");
            return Task.FromResult(result);
        }
    }
}