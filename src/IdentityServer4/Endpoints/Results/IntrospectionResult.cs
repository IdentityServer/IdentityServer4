// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Endpoints.Results
{
    public class IntrospectionResult : IEndpointResult
    {
        public Dictionary<string, object> Result { get; private set; }

        public IntrospectionResult(Dictionary<string, object> result)
        {
            Result = result;
        }
        
        public Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.SetNoCache();
            return context.HttpContext.Response.WriteJsonAsync(Result);
        }
    }
}