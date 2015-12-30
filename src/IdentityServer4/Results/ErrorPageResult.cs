using System;
using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    public class ErrorPageResult : RedirectToPageResult
    {
        public ErrorPageResult(string id)
            : base(Constants.RoutePaths.Error, id)
        {
        }
    }
}
