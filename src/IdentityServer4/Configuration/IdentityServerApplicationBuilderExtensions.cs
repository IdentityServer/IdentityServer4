// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class IdentityServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IdentityServerOptions>();
            app.UseCors(options.CorsOptions.CorsPolicyName);

            app.ConfigureCookies();
            app.UseMiddleware<AuthenticationMiddleware>();
            app.UseMiddleware<BaseUrlMiddleware>();
            app.UseMiddleware<IdentityServerMiddleware>();
            app.UseMiddleware<FederatedSignOutMiddleware>();

            return app;
        }
    }
}