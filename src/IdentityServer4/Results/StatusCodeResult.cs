using System.Threading.Tasks;
using System.Net;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    public class StatusCodeResult : IEndpointResult
    {
        public int StatusCode { get; private set; }

        public StatusCodeResult(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
        }

        public StatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCode;

            return Task.FromResult(0);
        }
    }
}