using IdentityServer4.Core.Results;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public interface IEndpoint
    {
        Task<IEndpointResult> ProcessAsync(IdentityServerContext context);
    }
}