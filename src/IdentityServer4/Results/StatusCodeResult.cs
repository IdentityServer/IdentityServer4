using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace IdentityServer4.Core.Results
{
    public class StatusCodeResult : IResult
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

        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            context.Response.StatusCode = StatusCode;

            return Task.FromResult(0);
        }
    }
}