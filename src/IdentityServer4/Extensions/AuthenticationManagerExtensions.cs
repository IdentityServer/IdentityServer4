// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Authentication
{
    public static class AuthenticationManagerExtensions
    {
        public static async Task SignInAsync(this AuthenticationManager manager, string sub, string name, params Claim[] claims)
        {
            var scheme = manager.HttpContext.RequestServices.GetRequiredService<IdentityServerOptions>().AuthenticationOptions.EffectiveAuthenticationScheme;

            var principal = IdentityServerPrincipal.Create(sub, name, claims);
            await manager.SignInAsync(scheme, principal);
        }
    }
}