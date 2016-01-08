// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Hosting;
using System;

namespace Microsoft.AspNet.Builder
{
    public static class IdentityServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder app)
        {
            app.UseCors(String.Empty);
            app.ConfigureCookies();
            app.UseMiddleware<BaseUrlMiddleware>();
            app.UseMiddleware<IdentityServerMiddleware>();
            
            return app;
        }
    }
}