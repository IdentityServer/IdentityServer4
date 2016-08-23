using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Tests.Validation
{
    public class TestResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private string _erroDescription;
        private TokenErrors _error;
        private readonly bool _sendError;

        public TestResourceOwnerPasswordValidator()
        { }

        public TestResourceOwnerPasswordValidator(TokenErrors error, string errorDescription = null)
        {
            _sendError = true;
            _error = error;
            _erroDescription = errorDescription;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (_sendError)
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