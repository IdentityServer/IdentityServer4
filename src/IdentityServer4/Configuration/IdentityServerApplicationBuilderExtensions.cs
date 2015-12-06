using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Hosting;

namespace Microsoft.AspNet.Builder
{
    public static class IdentityServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder app)
        {
            app.UseMiddleware<BaseUrlMiddleware>();
            app.UseMiddleware<IdentityServerMiddleware>();
            
            return app;
        }
    }
}