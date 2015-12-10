using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public class BaseUrlMiddleware
    {
        private readonly IdentityServerContext _context;
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;

        public BaseUrlMiddleware(RequestDelegate next, IdentityServerContext context, IdentityServerOptions options)
        {
            _next = next;
            _context = context;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            var origin = _options.PublicOrigin;
            if (origin.IsMissing())
            {
                origin = context.Request.Scheme + "://" + request.Host.Value;
            }

            _context.SetHost(origin);
            _context.SetBasePath(request.PathBase.Value.RemoveTrailingSlash());

            await _next(context);
        }
    }
}