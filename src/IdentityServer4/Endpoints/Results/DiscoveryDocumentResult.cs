// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Hosting;
using IdentityServer4.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    public class DiscoveryDocumentResult : IEndpointResult
    {
        public DiscoveryDocument Document { get; private set; }
        public Dictionary<string, object> CustomEntries { get; private set; }

        public DiscoveryDocumentResult(DiscoveryDocument document, Dictionary<string, object> customEntries)
        {
            Document = document;
            CustomEntries = customEntries;
        }
        
        public Task ExecuteAsync(HttpContext context)
        {
            if (CustomEntries != null && CustomEntries.Any())
            {
                var jobject = JObject.FromObject(Document);

                foreach (var item in CustomEntries)
                {
                    JToken token;
                    if (jobject.TryGetValue(item.Key, out token))
                    {
                        throw new Exception("Item does already exist - cannot add it via a custom entry: " + item.Key);
                    }

                    if (item.Value.GetType().GetTypeInfo().IsClass)
                    {
                        jobject.Add(new JProperty(item.Key, JToken.FromObject(item.Value)));
                    }
                    else
                    {
                        jobject.Add(new JProperty(item.Key, item.Value));
                    }
                }

                return context.Response.WriteJsonAsync(jobject);
            }

            return context.Response.WriteJsonAsync(Document);
        }
    }
}