using System.Collections.Generic;
using System.Threading;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Endpoint = Microsoft.AspNetCore.Http.Endpoint;

namespace IdentityServer4.Configuration
{
    internal class IdentityServerEndpointDataSource : EndpointDataSource
    {
        private readonly IdentityServerOptions _options;

        private readonly RequestDelegate _requestDelegate;

        private readonly IReadOnlyList<Endpoint> _endpoints;

        public IdentityServerEndpointDataSource(IEnumerable<IdentityServer4.Hosting.Endpoint> endpoints, IdentityServerOptions options)
        {
            _options = options;
            _requestDelegate = CreateRequestDelegate(options);
            _endpoints = ConvertEndpoints(endpoints);
        }

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);

        private IReadOnlyList<Endpoint> ConvertEndpoints(IEnumerable<IdentityServer4.Hosting.Endpoint> endpoints)
        {
            var result = new List<Endpoint>();

            foreach (var endpoint in endpoints)
            {
                if (!_options.Endpoints.IsEndpointEnabled(endpoint))
                {
                    continue; // We don't even add disabled endpoints to the collection.
                }

                var pattern = RoutePatternFactory.Parse(endpoint.Path);

                var builder = new RouteEndpointBuilder(_requestDelegate, pattern, order: 0) { DisplayName = endpoint.Name };

                builder.Metadata.Add(new RouteNameMetadata(endpoint.Name));
                builder.Metadata.Add(new EndpointNameMetadata(endpoint.Name));
                builder.Metadata.Add(new IdentityServerEndpointMetadata(endpoint));
                builder.Metadata.Add(new EnableCorsAttribute(_options.Cors.CorsPolicyName));

                // TODO: Add HTTP method metadata.
                // builder.Metadata.Add(new HttpMethodMetadata(endpoint.Methods));

                result.Add(builder.Build());
            }

            return result;
        }

        private static RequestDelegate CreateRequestDelegate(IdentityServerOptions options)
        {
            return context =>
            {
                BaseUrlMiddleware.Handle(context, options);

                var session = context.RequestServices.GetRequiredService<IUserSession>();
                var events = context.RequestServices.GetRequiredService<IEventService>();
                var logger = context.RequestServices.GetRequiredService<ILogger<IdentityServer4.Hosting.Endpoint>>();

                return IdentityServerMiddleware.Handle(context, session, events, logger, null, GetHandler);
            };
        }

        private static IEndpointHandler GetHandler(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var metadata = endpoint.Metadata.GetMetadata<IdentityServerEndpointMetadata>().Endpoint;
            return context.RequestServices.GetService(metadata.Handler) as IEndpointHandler;
        }
    }
}