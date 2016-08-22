using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Tests.Validation
{
    public class TestResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context.UserName == context.Password)
            {
                context.Result = new GrantValidationResult(context.UserName, "password");
            }

            return Task.FromResult(0);
        }
    }
}