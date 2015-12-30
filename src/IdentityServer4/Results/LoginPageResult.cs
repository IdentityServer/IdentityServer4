using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Results
{
    public class LoginPageResult : RedirectToPageResult
    {
        public LoginPageResult(string id)
            : base(Constants.RoutePaths.Login, id)
        {
        }
    }
}
