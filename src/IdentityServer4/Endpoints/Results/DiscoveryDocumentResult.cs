// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    public class DiscoveryDocumentResult : IEndpointResult
    {
        public Dictionary<string, object> Entries { get; }

        public DiscoveryDocumentResult(Dictionary<string, object> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            Entries = entries;
        }
        
        public Task ExecuteAsync(HttpContext context)
        {
                var jobject = ObjectSerializer.ToJObject(Entries);
                return context.Response.WriteJsonAsync(jobject);
        }
    }
}