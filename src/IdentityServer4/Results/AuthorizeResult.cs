using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    abstract class AuthorizeResult : IEndpointResult
    {
        public AuthorizeResponse Response { get; private set; }

        public AuthorizeResult(AuthorizeResponse response)
        {
            Response = response;
        }

        public abstract Task ExecuteAsync(IdentityServerContext context);
    }
}
