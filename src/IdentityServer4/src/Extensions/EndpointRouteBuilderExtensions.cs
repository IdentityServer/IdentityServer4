using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Provide extensions for IdentityServer endpoint routing.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that handles IdentityServer4 requests.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
        public static IEndpointConventionBuilder MapIdentityServer(
            this IEndpointRouteBuilder endpoints)
        {
            var dataSource = endpoints.ServiceProvider.GetRequiredService<IdentityServerEndpointDataSource>();
            endpoints.DataSources.Add(dataSource);

            var options = endpoints.ServiceProvider.GetRequiredService<IdentityServerOptions>();
            options.UseEndpointRouting = true;

            return dataSource;
        }
    }
}
