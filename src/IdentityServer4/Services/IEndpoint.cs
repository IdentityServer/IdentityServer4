using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    public interface IEndpoint
    {
        Task ProcessAsync(HttpContext context);
    }
}