// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Hosting
{
    public static class CookieMiddlewareExtensions
    {
        public static void ConfigureCookies(this IApplicationBuilder app)
        {
            var idSvrOptions = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();
            if (idSvrOptions.Endpoints.EnableAuthorizeEndpoint &&
                idSvrOptions.AuthenticationOptions.AuthenticationScheme.IsMissing())
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = idSvrOptions.AuthenticationOptions.EffectiveAuthenticationScheme,
                    AutomaticAuthenticate = true,
                    SlidingExpiration = false,
                    ExpireTimeSpan = Constants.DefaultCookieTimeSpan,
                    CookieName = Constants.DefaultCookieAuthenticationScheme,
                });
            }
        }
    }
}
