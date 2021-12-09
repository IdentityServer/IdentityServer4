using Bornlogic.IdentityServer.Host.Stores.Contracts;
using Bornlogic.IdentityServer.Validation;
using Bornlogic.IdentityServer.Validation.Contexts;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;

namespace Bornlogic.IdentityServer.Host
{
    public class MyResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IApplicationUserStore _users;
        
        public MyResourceOwnerPasswordValidator(IApplicationUserStore users)
        {
            _users = users;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (await _users.ValidateCredentials(context.UserName, context.Password))
            {
                var user = await _users.FindByUsername(context.UserName);
                context.Result = new GrantValidationResult(user.Id ?? throw new ArgumentException("Subject ID not set", nameof(user.Id)), OidcConstants.AuthenticationMethods.Password, DateTime.UtcNow, await _users.GetClaims(user));
            }
        }
    }
}