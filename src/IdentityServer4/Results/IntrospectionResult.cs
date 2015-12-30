using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
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
            return context.HttpContext.Response.WriteJsonAsync(Result);
        }
    }
}