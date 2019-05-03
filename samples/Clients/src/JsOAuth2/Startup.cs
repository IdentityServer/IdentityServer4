using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace JsOAuth2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}