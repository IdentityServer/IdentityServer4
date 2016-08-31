using Host.Models;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Host.Controllers
{
    public class LogoutController : Controller
    {
        private readonly IUserInteractionService _interaction;

        public LogoutController(IUserInteractionService interaction)
        {
            _interaction = interaction;
        }

        public IActionResult Index(string logoutId)
        {
            ViewData["logoutId"] = logoutId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string logoutId)
        {
            await HttpContext.Authentication.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = logout?.ClientId,
                SignOutIframeUrl = logout?.SignOutIFrameUrl
            };

            return View("LoggedOut", vm);
        }
    }
}