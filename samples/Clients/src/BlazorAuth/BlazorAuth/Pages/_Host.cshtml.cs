using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorAuth.Pages
{
    public class HostModel : PageModel
    {
        [BindProperty(SupportsGet = true)] public string RedirectUri { get; set; }
        [BindProperty(SupportsGet = true)] public bool LogIn { get; set; } = false;
        [BindProperty(SupportsGet = true)] public bool LogOut { get; set; } = false;

        public async Task OnGetAsync()
        {
            if (LogIn)
            {
                var prop = new AuthenticationProperties {RedirectUri = RedirectUri};
                string auth = HttpContext.Request.Headers["Authorization"];
                await HttpContext.ChallengeAsync(prop);
            }

            if (LogOut)
            {
                var prop = new AuthenticationProperties {RedirectUri = RedirectUri};
                await HttpContext.SignOutAsync("Cookies");
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, prop);
            }
        }
    }
}