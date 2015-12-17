using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public class DiscoveryDocumentResult : IResult
    {
        public DiscoveryDocument Document { get; private set; }
        public Dictionary<string, object> CustomEntries { get; private set; }

        public DiscoveryDocumentResult(DiscoveryDocument document, Dictionary<string, object> customEntries)
        {
            Document = document;
            CustomEntries = customEntries;
        }
        
        public Task ExecuteAsync(HttpContext context, ILogger logger)
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

                    jobject.Add(new JProperty(item.Key, item.Value));

                    return context.Response.WriteJsonAsync(jobject);
                }
            }

            return context.Response.WriteJsonAsync(Document);
        }
    }
}