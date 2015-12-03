using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public interface IResult
    {
        Task ExecuteAsync(HttpContext context);
    }
}