using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public interface IEndpoint
    {
        Task ProcessAsync(HttpContext context);
    }
}