using IdentityServer4.Core.Hosting;

namespace Microsoft.AspNet.Builder
{
    public static class IdentityServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder app)
        {
            app.ConfigureCookies();
            app.UseMiddleware<BaseUrlMiddleware>();
            app.UseMiddleware<IdentityServerMiddleware>();
            
            return app;
        }
    }
}