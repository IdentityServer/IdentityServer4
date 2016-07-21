using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Validation
{
    public class TestResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task<GrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request)
        {
            if (userName == password)
            {
                var result = new GrantValidationResult(request.UserName, "password");
                return Task.FromResult(result);
            }
            else
            {
                var result = new GrantValidationResult("Username and/or password incorrect");
                return Task.FromResult(result);
            }
        }
    }
}