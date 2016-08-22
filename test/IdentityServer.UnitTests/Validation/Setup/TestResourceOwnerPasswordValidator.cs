using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityModel;
using IdentityServer4.Extensions;

namespace IdentityServer4.Tests.Validation
{
    public class TestResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private string _erroDescription;
        private string _error;

        public TestResourceOwnerPasswordValidator(string error = null, string errorDescription = null)
        {
            _error = error;
            _erroDescription = errorDescription;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (_error.IsPresent())
            {
                context.Result = new GrantValidationResult(_error, _erroDescription);
                return Task.FromResult(0);
            }

            if (context.UserName == context.Password)
            {
                context.Result = new GrantValidationResult(context.UserName, "password");
            }

            return Task.FromResult(0);
        }
    }
}