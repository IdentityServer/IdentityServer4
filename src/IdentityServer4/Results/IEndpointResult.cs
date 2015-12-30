using IdentityServer4.Core.Hosting;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public interface IEndpointResult
    {
        Task ExecuteAsync(IdentityServerContext context);
    }
}