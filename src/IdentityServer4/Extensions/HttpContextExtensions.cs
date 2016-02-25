using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Configuration;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Core.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Clear authentication cookie.
        /// </summary>
        /// <param name="context"></param>
        public static async void ClearAuthenticationCookies(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var options = context.ApplicationServices.GetRequiredService<IdentityServerOptions>();
            
            await context.Authentication.SignOutAsync(options.AuthenticationOptions.EffectivePrimaryAuthenticationScheme);
        }
    }
}
