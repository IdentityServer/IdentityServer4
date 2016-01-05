using IdentityServer4.Core;
using IdentityServer4.Core.Services;
using Microsoft.AspNet.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Host.UI.Consent
{
    public class ConsentController : Controller
    {
        private readonly ConsentInteraction _consentInteraction;

        public ConsentController(ConsentInteraction consentInteraction)
        {
            _consentInteraction = consentInteraction;
        }

        [HttpGet(Constants.RoutePaths.Consent, Name = "Consent")]
        public async Task<IActionResult> Index(string id)
        {
            if (id != null)
            {
                var request = await _consentInteraction.GetRequestAsync(id);
                if (request != null)
                {
                    return new ConsentResult(id, new IdentityServer4.Core.Models.ConsentResponse {
                        ScopesConsented = request.ScopesRequested,
                        RememberConsent = true
                    });
                }
            }

            return View("Error");
        }
    }
}
