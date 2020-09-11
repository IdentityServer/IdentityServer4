using IdentityServer4.Configuration;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HttpEndpoint = Microsoft.AspNetCore.Http.Endpoint;

namespace IdentityServer4.Hosting
{
    internal sealed class IdentityServerEndpointDataSource : EndpointDataSource, IEndpointConventionBuilder
    {
        private readonly IEnumerable<Endpoint> _idsendpoints;
        private readonly List<Action<EndpointBuilder>> _conventions;
        private readonly object _locker;
        private readonly IdentityServerOptions _options;
        private readonly ILogger<Item> _logger;
        private IReadOnlyList<HttpEndpoint> _endpoints;

        private struct Item { }

        public IdentityServerEndpointDataSource(IEnumerable<Endpoint> endpoints, IdentityServerOptions options, ILoggerFactory logger)
        {
            _idsendpoints = endpoints;
            _conventions = new List<Action<EndpointBuilder>>();
            _locker = new object();
            _options = options;
            _logger = logger.CreateLogger<Item>();
        }

        public override IReadOnlyList<HttpEndpoint> Endpoints
        {
            get
            {
                Initialize();
                Debug.Assert(_endpoints != null);
                return _endpoints;
            }
        }

        private void Initialize()
        {
            if (_endpoints == null)
            {
                lock (_locker)
                {
                    if (_endpoints == null)
                    {
                        UpdateEndpoints();
                    }
                }
            }
        }

        private async Task HandleEndpoint(HttpContext context, Endpoint endpoint)
        {
            var events = context.RequestServices.GetRequiredService<IEventService>();
            IEndpointHandler handler;

            if (_options.Endpoints.IsEndpointEnabled(endpoint))
            {
                if (context.RequestServices.GetService(endpoint.Handler) is IEndpointHandler handler2)
                {
                    _logger.LogDebug("Endpoint enabled: {endpoint}, successfully created handler: {endpointHandler}", endpoint.Name, endpoint.Handler.FullName);
                    handler = handler2;
                }
                else
                {
                    _logger.LogDebug("Endpoint enabled: {endpoint}, failed to create handler: {endpointHandler}", endpoint.Name, endpoint.Handler.FullName);
                    context.Response.StatusCode = 500;
                    return;
                }
            }
            else
            {
                _logger.LogWarning("Endpoint disabled: {endpoint}", endpoint.Name);
                context.Response.StatusCode = 404;
                return;
            }

            try
            {
                _logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());

                var result = await handler.ProcessAsync(context);

                if (result != null)
                {
                    _logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                    await result.ExecuteAsync(context);
                }
            }
            catch (Exception ex)
            {
                await events.RaiseAsync(new UnhandledExceptionEvent(ex));
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
                throw;
            }
        }

        private void UpdateEndpoints()
        {
            lock (_locker)
            {
                var endpoints = new List<HttpEndpoint>();
                var order = 0;

                foreach (var endpoint in _idsendpoints)
                {
                    RequestDelegate handler = context => HandleEndpoint(context, endpoint);
                    var pattern = RoutePatternFactory.Parse(endpoint.Path.Value);
                    var builder = new RouteEndpointBuilder(handler, pattern, ++order);
                    _conventions.ForEach(a => a.Invoke(builder));
                    builder.DisplayName = endpoint.Name;
                    endpoints.Add(builder.Build());
                }

                _endpoints = endpoints;
            }
        }

        public void Add(Action<EndpointBuilder> convention)
        {
            _conventions.Add(convention);
        }

        public override IChangeToken GetChangeToken()
        {
            return NullChangeToken.Singleton;
        }
    }
}
