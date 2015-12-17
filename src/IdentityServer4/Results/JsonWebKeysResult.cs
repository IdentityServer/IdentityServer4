using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public class JsonWebKeysResult : IResult
    {
        public IEnumerable<JsonWebKey> WebKeys { get; private set; }

        public JsonWebKeysResult(IEnumerable<JsonWebKey> webKeys)
        {
            WebKeys = webKeys;
        }
        
        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            return context.Response.WriteJsonAsync(new { keys = WebKeys });
        }
    }
}