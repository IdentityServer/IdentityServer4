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

        //M.S. For Development Only: Authentication Properties made public for demonstration only!
        public static AuthenticationProperties properties;

        public async Task OnGetAsync()
        {
            if (LogIn)
            {
                properties = new AuthenticationProperties {RedirectUri = RedirectUri};
                await HttpContext.ChallengeAsync(properties);
            }

            if (LogOut)
            {
                properties = new AuthenticationProperties {RedirectUri = RedirectUri};
                await HttpContext.SignOutAsync("Cookies");
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
            }
        }
    }
}