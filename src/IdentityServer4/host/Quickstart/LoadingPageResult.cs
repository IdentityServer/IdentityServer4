using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Quickstart.UI
{
    public class LoadingPageResult : ViewResult
    {
        public LoadingPageResult(string viewName, string redirectUri)
        {
            ViewName = viewName;
            ViewData.Model = new RedirectViewModel { RedirectUrl = redirectUri };
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers["Location"] = "";

            return base.ExecuteResultAsync(context);
        }
    }
}