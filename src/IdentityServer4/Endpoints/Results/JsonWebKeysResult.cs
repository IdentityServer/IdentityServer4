// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints.Results
{
    public class JsonWebKeysResult : IEndpointResult
    {
        public IEnumerable<JsonWebKey> WebKeys { get; private set; }

        public JsonWebKeysResult(IEnumerable<JsonWebKey> webKeys)
        {
            WebKeys = webKeys;
        }
        
        public Task ExecuteAsync(IdentityServerContext context)
        {
            return context.HttpContext.Response.WriteJsonAsync(new { keys = WebKeys });
        }
    }
}