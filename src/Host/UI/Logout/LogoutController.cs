using IdentityServer4.Core;
using IdentityServer4.Core.Services;
using Microsoft.AspNet.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using Microsoft.Extensions.Logging;

namespace Host.UI.Logout
{
    public class LogoutController : Controller
    {
        private readonly ILogger<LogoutController> _logger;
        private readonly IdentityServerContext _context;
        private readonly IClientStore _clientStore;
        private readonly SignOutInteraction _signOutInteraction;
        private readonly IMessageStore<SignOutRequest> _messageStore;

        public LogoutController(
            ILogger<LogoutController> logger, 
            IdentityServerContext context,
            IClientStore clientStore,
            SignOutInteraction signOutInteraction,
            IMessageStore<SignOutRequest> messageStore)
        {
            _signOutInteraction = signOutInteraction;
            _logger = logger;
            _context = context;
            _clientStore = clientStore;
            _messageStore = messageStore;
        }

        [HttpGet(Constants.RoutePaths.Logout, Name = "Logout")]
        public async Task<IActionResult> Index(string id)
        {
            if (User == null || User.Identity.IsAuthenticated == false)
            {
                // user is already logged out, so just trigger logout cleanup
                return await Submit(id);
            }

            var sub = User.GetSubjectId();
            _logger.LogInformation($"Logout prompt for subject: {sub}");

            if (_context.Options.AuthenticationOptions.RequireSignOutPrompt == false)
            {
                var signOutRequest = await _signOutInteraction.GetRequestAsync(id);
                if (!string.IsNullOrWhiteSpace(signOutRequest?.ClientId))
                {
                    var client = await _clientStore.FindClientByIdAsync(signOutRequest.ClientId);
                    if (client != null && client.RequireSignOutPrompt)
                    {
                        _logger.LogInformation($"SignOutRequest present (from client {signOutRequest.ClientId}) but RequireSignOutPrompt is true, rendering logout prompt");

                        return await RenderPromptLogout(id);
                    }

                    _logger.LogInformation($"SignOutRequest present (from client {signOutRequest.ClientId}) and RequireSignOutPrompt is false, performing logout");
                    return await Submit(id);
                }

                _logger.LogInformation("EnableSignOutPrompt set to true, rendering logout");
            }
            
            _logger.LogInformation("RequireSignOutPrompt set to true, rendering logout");

            return await RenderPromptLogout(id);
        }
        
        [HttpPost(Constants.RoutePaths.Logout)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string signOutId)
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                var sub = User.GetSubjectId();
                _logger.LogInformation($"Logout requested for subject: {sub}");
            }

            _logger.LogInformation("Clearing cookies");
            HttpContext.ClearAuthenticationCookies();

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var vm = new LoggedOutViewModel();

            if (!string.IsNullOrWhiteSpace(signOutId))
            {
                var message = await _messageStore.ReadAsync(signOutId);

                if (!string.IsNullOrWhiteSpace(message.ResponseUrl))
                {
                    HttpContext.Response.Redirect(message.ResponseUrl);
                }

                var client = await _clientStore.FindClientByIdAsync(message.Data.ClientId);
                
                vm.ClientName = client.ClientName;
                vm.SignOutIFrameUrls = client.PostLogoutRedirectUris;
                vm.ReturnInfo = new ClientReturnInfo
                {
                    ClientId = client.ClientId,
                    Uri = message.ResponseUrl
                };
            }

            return View("LoggedOut", vm);
        }

        private async Task<IActionResult> RenderPromptLogout(string id)
        {
            return View(new LogoutViewModel { SignOutId = id });
        }
    }
}
