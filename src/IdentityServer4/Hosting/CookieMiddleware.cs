// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#pragma warning disable 1591

namespace IdentityServer4.Hosting
{
    public static class CookieMiddlewareExtensions
    {
        public static void ConfigureCookies(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CookieMiddlewareExtensions).FullName);
            var options = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();

            // only do stuff with cookies if we're showing UI
            if (options.Endpoints.EnableAuthorizeEndpoint)
            {
                if (options.Authentication.AuthenticationScheme.IsMissing())
                {
                    logger.LogDebug("Using built-in CookieAuthentication middleware with scheme: {authenticationScheme}", options.Authentication.EffectiveAuthenticationScheme);
                    app.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationScheme = options.Authentication.EffectiveAuthenticationScheme,
                        AutomaticAuthenticate = true,
                        SlidingExpiration = options.Authentication.CookieSlidingExpiration,
                        ExpireTimeSpan = options.Authentication.CookieLifetime,
                        CookieName = IdentityServerConstants.DefaultCookieAuthenticationScheme,
                        LoginPath = ExtractLocalUrl(options.UserInteraction.LoginUrl),
                        LogoutPath = ExtractLocalUrl(options.UserInteraction.LogoutUrl),
                        ReturnUrlParameter = options.UserInteraction.LoginReturnUrlParameter
                    });

                    logger.LogDebug("Adding CookieAuthentication middleware for external authentication with scheme: {authenticationScheme}", IdentityServerConstants.ExternalCookieAuthenticationScheme);
                    app.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                        AutomaticAuthenticate = false,
                        AutomaticChallenge = false
                    });
                }
                else
                {
                    logger.LogDebug("Using hosting application's CookieAuthentication middleware with scheme: {authenticationScheme}", options.Authentication.EffectiveAuthenticationScheme);
                }

                app.UseMiddleware<AuthenticationMiddleware>();
                app.UseMiddleware<FederatedSignOutMiddleware>();
            }
        }

        private static string ExtractLocalUrl(string url)
        {
            if (url.IsLocalUrl())
            {
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(1);
                }

                return url;
            }

            return null;
        }
    }
}
