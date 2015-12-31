using IdentityServer4.Core.Services;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;

namespace UnitTests.Common
{
    public class FakeUserService : IUserService
    {
        public Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

        public Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.FromResult(0);
        }

        public Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

        public Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

        public Task SignOutAsync(SignOutContext context)
        {
            return Task.FromResult(0);
        }
    }
}
