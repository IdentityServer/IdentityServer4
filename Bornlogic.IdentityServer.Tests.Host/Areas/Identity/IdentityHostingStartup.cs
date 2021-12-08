using Bornlogic.IdentityServer.Tests.Host.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace Bornlogic.IdentityServer.Tests.Host.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {

            });
        }
    }
}