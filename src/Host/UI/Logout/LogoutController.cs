using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Host.UI.Logout
{
    public class LogoutController : Controller
    {
        public LogoutController()
        {
        }

        [HttpGet("ui/logout", Name = "Logout")]
        public IActionResult Index(string returnUrl)
        {
            if (returnUrl != null && !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = null;
            }
            return View(new LogoutViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost("ui/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(LogoutViewModel model)
        {
            await HttpContext.Authentication.SignOutAsync(Constants.PrimaryAuthenticationType);

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var vm = new LoggedOutViewModel()
            {
            };
            return View("LoggedOut", vm);
        }
    }
}
