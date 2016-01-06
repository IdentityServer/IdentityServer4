using IdentityServer4.Core;
using IdentityServer4.Core.Services;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Host.UI.Consent
{
    public class ConsentController : Controller
    {
        private readonly ILogger<ConsentController> _logger;
        private readonly IClientStore _clientStore;
        private readonly ConsentInteraction _consentInteraction;
        private readonly IScopeStore _scopeStore;

        public ConsentController(
            ILogger<ConsentController> logger,
            ConsentInteraction consentInteraction,
            IClientStore clientStore,
            IScopeStore scopeStore)
        {
            _logger = logger;
            _consentInteraction = consentInteraction;
            _clientStore = clientStore;
            _scopeStore = scopeStore;
        }

        [HttpGet(Constants.RoutePaths.Consent, Name = "Consent")]
        public async Task<IActionResult> Index(string id)
        {
            if (id != null)
            {
                var request = await _consentInteraction.GetRequestAsync(id);
                if (request != null)
                {
                    var client = await _clientStore.FindClientByIdAsync(request.ClientId);
                    if (client != null)
                    {
                        var scopes = await _scopeStore.FindScopesAsync(request.ScopesRequested);
                        if (scopes != null && scopes.Any())
                        {
                            var vm = new ConsentViewModel(id, request, client, scopes);
                            return View("Index", vm);
                        }
                        else
                        {
                            _logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x,y)=>x + ", " + y));
                        }
                    }
                    else
                    {
                        _logger.LogError("Invalid client id: {0}", request.ClientId);
                    }
                }
                else
                {
                    _logger.LogError("No consent request matching id: {0}", id);
                }
            }
            else
            {
                _logger.LogError("No id passed");
            }

            return View("Error");
        }

        //[HttpPost(Constants.RoutePaths.Consent)]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index(ConsentInputModel model)
        //{

        //    if (id != null)
        //    {
        //        var request = await _consentInteraction.GetRequestAsync(id);
        //        if (request != null)
        //        {
        //            return new ConsentResult(id, new IdentityServer4.Core.Models.ConsentResponse
        //            {
        //                ScopesConsented = request.ScopesRequested,
        //                RememberConsent = true
        //            });
        //        }
        //    }

        //    return View("Error");
        //}
    }
}
