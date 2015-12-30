using IdentityServer4.Core.Hosting;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public interface IEndpointResult
    {
        Task ExecuteAsync(IdentityServerContext context);
    }
}