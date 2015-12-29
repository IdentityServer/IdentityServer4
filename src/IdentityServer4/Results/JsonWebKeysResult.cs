using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
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