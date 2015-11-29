using IdentityServer4.Core.Services;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public class TokenEndpoint : IEndpoint
    {
        public Task ProcessAsync(HttpContext context)
        {
            // read input params

            // send to validator

            // send validation result to response generator

            // write out response
            
            return Task.FromResult(0);
        }
    }
}