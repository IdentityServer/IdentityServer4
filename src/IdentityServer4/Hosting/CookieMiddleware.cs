// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting
{
    public static class CookieMiddlewareExtensions
    {
        public static void ConfigureCookies(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CookieMiddlewareExtensions).FullName);

            var idSvrOptions = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();

            // only do stuff with cookies if we're showing UI
            if (idSvrOptions.Endpoints.EnableAuthorizeEndpoint)
            {
                if (idSvrOptions.AuthenticationOptions.AuthenticationScheme.IsMissing())
                {
                    logger.LogDebug("AuthenticationScheme is missing; configuring CookieAuthentication middleware");
                    app.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationScheme = idSvrOptions.AuthenticationOptions.EffectiveAuthenticationScheme,
                        AutomaticAuthenticate = true,
                        SlidingExpiration = false,
                        ExpireTimeSpan = Constants.DefaultCookieTimeSpan,
                        CookieName = IdentityServerConstants.DefaultCookieAuthenticationScheme,
                    });
                }
                else
                {
                    logger.LogDebug("AuthenticationScheme is configured; using hosting application's CookieAuthentication middleware");
                }

                app.UseMiddleware<AuthenticationMiddleware>();
                app.UseMiddleware<FederatedSignOutMiddleware>();
            }
        }
    }
}
