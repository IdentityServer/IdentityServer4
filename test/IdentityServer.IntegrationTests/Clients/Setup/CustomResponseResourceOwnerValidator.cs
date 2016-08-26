using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Tests.Clients
{
    public class CustomResponseResourceOwnerValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var response = new Dictionary<string, object>
            {
                { "string_value", "some_string" },
                { "int_value", 42 },
                { "dto",  CustomResponseDto.Create }
            };

            if (context.UserName == context.Password)
            {
                context.Result = new GrantValidationResult(context.UserName, "password", customResponse: response);
            }
            else
            {
                context.Result = new GrantValidationResult(TokenErrors.InvalidGrant, "invalid_credential", response);
            }

            return Task.CompletedTask;
        }
    }
}