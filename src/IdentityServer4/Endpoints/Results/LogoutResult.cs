using IdentityServer4.Core.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints.Results
{
    class LogoutResult : RedirectToPageResult
    {
        public LogoutResult(string id) : base(Constants.RoutePaths.Logout, id)
        {
        }
    }
}
