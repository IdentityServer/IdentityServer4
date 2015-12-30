using IdentityServer4.Core.Results;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public interface IEndpoint
    {
        Task<IEndpointResult> ProcessAsync(IdentityServerContext context);
    }
}