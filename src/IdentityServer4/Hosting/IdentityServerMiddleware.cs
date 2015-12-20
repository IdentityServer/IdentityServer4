using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static IdentityServer4.Core.Constants;

namespace IdentityServer4.Core.Hosting
{
    public class IdentityServerMiddleware
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RequestDelegate _next;

        public IdentityServerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<IdentityServerMiddleware>();
            _loggerFactory = loggerFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            var router = context.RequestServices.GetRequiredService<IEndpointRouter>();

            var endpoint = router.Find(context);
            if (endpoint != null)
            {
                var result = await endpoint.ProcessAsync(context);

                if (result != null)
                {
                    await result.ExecuteAsync(context, _loggerFactory.CreateLogger(result.GetType().FullName));
                }

                return;
            }

            await _next(context);
        }
    }
}