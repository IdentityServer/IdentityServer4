using System.Linq;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Routing extension methods for adding IdentityServer
    /// </summary>
    public static class IdentityServerEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Adds endpoints for IdentityServer to the <see cref="IEndpointRouteBuilder"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
        public static void MapIdentityServer(this IEndpointRouteBuilder endpoints)
        {
            endpoints.ServiceProvider.Validate();

            var dataSource = endpoints.DataSources.OfType<IdentityServerEndpointDataSource>().FirstOrDefault();
            if (dataSource is null)
            {
                dataSource = endpoints.ServiceProvider.GetRequiredService<IdentityServerEndpointDataSource>();
                endpoints.DataSources.Add(dataSource);
            }
        }
    }
}
