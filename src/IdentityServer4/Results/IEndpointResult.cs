using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Results
{
    public interface IEndpointResult
    {
        Task ExecuteAsync(IdentityServerContext context);
    }
}