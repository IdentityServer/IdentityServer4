// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable 1591

namespace IdentityServer4.Hosting
{
    public static class CookieConfiguration
    {
        public static void ConfigureCookies(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CookieConfiguration).FullName);
            var options = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();

            if (options.Authentication.AuthenticationScheme.IsMissing())
            {
                logger.LogDebug("Using built-in cookie authentication service for sign-in ({authenticationScheme}) and external authentication ({externalAuthenticationScheme})", IdentityServerConstants.DefaultCookieAuthenticationScheme, IdentityServerConstants.ExternalCookieAuthenticationScheme);
            }
            else
            {
                logger.LogDebug("Using hosting application's CookieAuthentication middleware with scheme: {authenticationScheme}", options.Authentication.EffectiveAuthenticationScheme);

                // todo: right place for this config code?
                var authOptions = app.ApplicationServices.GetRequiredService<IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>();
                authOptions.Value.DefaultAuthenticateScheme = options.Authentication.EffectiveAuthenticationScheme;
                authOptions.Value.DefaultChallengeScheme = options.Authentication.EffectiveAuthenticationScheme;
                authOptions.Value.DefaultSignInScheme = options.Authentication.EffectiveAuthenticationScheme;
            }            
        }
    }
}