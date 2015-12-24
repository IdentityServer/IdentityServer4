using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public static class CookieMiddlewareExtensions
    {
        public static void ConfigureCookies(this IApplicationBuilder app)
        {
            var idSvrOptions = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();
            if (idSvrOptions.Endpoints.EnableAuthorizeEndpoint &&
                idSvrOptions.AuthenticationOptions.PrimaryAuthenticationScheme.IsMissing())
            {
                app.UseCookieAuthentication(options =>
                {
                    options.AuthenticationScheme = idSvrOptions.AuthenticationOptions.EffectivePrimaryAuthenticationScheme;
                });
            }
        }
    }
}
