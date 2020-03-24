using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.Extensions.Logging;

namespace Host.Extensions
{
    public class HostProfileService : TestUserProfileService
    {
        public HostProfileService(TestUserStore users, ILogger<TestUserProfileService> logger) : base(users, logger)
        {
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return base.GetProfileDataAsync(context);
        }

        public override Task IsActiveAsync(IsActiveContext context)
        {
            return base.IsActiveAsync(context);
        }
    }
}