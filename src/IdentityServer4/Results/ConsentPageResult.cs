using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Results
{
    public class ConsentPageResult : RedirectToPageResult
    {
        public ConsentPageResult(string id)
            : base(Constants.RoutePaths.Oidc.Consent, id)
        {
        }
    }
}
