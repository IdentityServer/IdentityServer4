// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;

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
                    options.AutomaticAuthenticate = true;
                });
            }
        }
    }
}
