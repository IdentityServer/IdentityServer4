using IdentityServer4.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Validation
{
    public class TestResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task<CustomGrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request)
        {
            if (userName == password)
            {
                var result = new CustomGrantValidationResult(request.UserName, "password");
                return Task.FromResult(result);
            }
            else
            {
                var result = new CustomGrantValidationResult("Username and/or password incorrect");
                return Task.FromResult(result);
            }
        }
    }
}