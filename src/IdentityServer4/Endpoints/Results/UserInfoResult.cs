// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Endpoints.Results
{
    internal class UserInfoResult : IEndpointResult
    {
        public Dictionary<string, object> Claims;

        public UserInfoResult(Dictionary<string, object> claims)
        {
            Claims = claims;
        }

        public async Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.SetNoCache();
            await context.HttpContext.Response.WriteJsonAsync(Claims);
        }
    }
}