using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// IdentityServer extension methods for <see cref="IEndpointRouteBuilder"/>
    /// </summary>
    public static class IdentityServerEndpointConventionExtensions
    {
        /// <summary>
        /// Adds IdentityServer endpoints to the <see cref="IEndpointRouteBuilder"/> with the specified options.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the IdentityServer endpoints to.</param>
        /// <param name="options">An <see cref="IdentityServerMiddlewareOptions"/> used to configure the IdentityServer.</param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapIdentityServer(
            this IEndpointRouteBuilder endpoints, 
            IdentityServerMiddlewareOptions options = null)
        {
            if (endpoints is null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var pipeline = endpoints.CreateApplicationBuilder()
                .UseIdentityServer(options)
                .Build();

            var patterns = endpoints.ServiceProvider.GetServices<IdentityServer4.Hosting.Endpoint>();

            foreach (var pattern in patterns)
            {
                endpoints.Map(pattern.Path, pipeline);
            }

            return endpoints;
        }
    }
}
