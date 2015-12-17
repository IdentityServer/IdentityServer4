using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class IntrospectionResult : IResult
    {
        public Dictionary<string, object> Result { get; private set; }

        public IntrospectionResult(Dictionary<string, object> result)
        {
            Result = result;
        }
        
        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            logger.LogInformation("Returning introspection response.");

            return context.Response.WriteJsonAsync(Result);
        }
    }
}