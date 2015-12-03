using IdentityServer4.Core.Results;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public interface IEndpoint
    {
        Task<IResult> ProcessAsync(HttpContext context);
    }
}