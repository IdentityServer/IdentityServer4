using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services.InMemory
{
    public class InMemoryResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly List<InMemoryUser> _users;

        public InMemoryResourceOwnerPasswordValidator(List<InMemoryUser> users)
        {
            _users = users;
        }

        public Task<GrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request)
        {
            var query =
                from u in _users
                where u.Username == userName && u.Password == password
                select u;

            var user = query.SingleOrDefault();
            if (user != null)
            {
                return Task.FromResult(new GrantValidationResult(user.Subject, "password", user.Claims));
            }

            return Task.FromResult(new GrantValidationResult("Invalid username or password"));
        }
    }
}
