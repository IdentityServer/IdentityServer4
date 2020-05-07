using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Logging;

namespace Host.Extensions
{
    public class HostProfileService : TestUserProfileService
    {
        public HostProfileService(TestUserStore users, ILogger<TestUserProfileService> logger) : base(users, logger)
        {
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            var transaction = context.RequestedResources.ParsedScopes.FirstOrDefault(x => x.Name == "transaction");
            if (transaction?.ParameterValue != null)
            {
                context.IssuedClaims.Add(new Claim("transaction_id", transaction.ParameterValue));
            }
        }
    }
}