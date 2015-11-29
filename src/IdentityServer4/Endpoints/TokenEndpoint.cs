using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public class TokenEndpoint
    {
        public Task ProcessAsync()
        {
            // read input params

            // send to validator

            // send validation result to response generator

            // write out response
            
            return Task.FromResult(0);
        }
    }
}