using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    public class ConsentPageResult : IEndpointResult
    {
        public ConsentPageResult()
        {
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            return Task.FromResult(0);
        }
    }
}
