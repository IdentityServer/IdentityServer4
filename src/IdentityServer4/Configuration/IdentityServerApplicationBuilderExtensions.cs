using IdentityServer4.Core.Configuration;

namespace Microsoft.AspNet.Builder
{
    public static class IdentityServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder app)
        {
            app.UseMiddleware<IdentityServerMiddleware>();
            
            return app;
        }
    }
}