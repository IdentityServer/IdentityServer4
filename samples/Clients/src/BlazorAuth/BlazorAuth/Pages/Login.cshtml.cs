using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OktaBlazorAspNetCoreServerSide.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string RedirectUri { get; set; }
        public async Task OnGetAsync()
        {
            var prop = new AuthenticationProperties
            {
                RedirectUri = RedirectUri
            };
            string auth = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
            {
                await HttpContext.ChallengeAsync(prop);
            }
        }
    }
}