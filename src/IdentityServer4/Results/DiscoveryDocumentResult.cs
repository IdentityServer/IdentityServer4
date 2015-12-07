using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public class DiscoveryDocumentResult : IResult
    {
        public DiscoveryDocument Document { get; private set; }

        public DiscoveryDocumentResult(DiscoveryDocument document)
        {
            Document = document;
        }
        
        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            return context.Response.WriteJsonAsync(Document);
        }
    }
}