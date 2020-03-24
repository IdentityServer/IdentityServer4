using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Quickstart.UI
{
    public static class Extensions
    {
        /// <summary>
        /// Determines whether the client is configured to use PKCE.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="client_id">The client identifier.</param>
        /// <returns></returns>
        public static async Task<bool> IsPkceClientAsync(this IClientStore store, string client_id)
        {
            if (!string.IsNullOrWhiteSpace(client_id))
            {
                var client = await store.FindEnabledClientByIdAsync(client_id);
                return client.IsPkceClient();
            }

            return false;
        }
        
        /// <summary>
        /// Checks is client is PKCE.
        /// </summary>
        /// <returns></returns>
        public static bool IsPkceClient(this Client client)
        {
            return client?.RequirePkce == true;
        }

        public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = "";
            
            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}
