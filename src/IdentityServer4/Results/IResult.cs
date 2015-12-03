using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public interface IResult
    {
        Task ExecuteAsync(HttpContext context, ILogger logger);
    }
}