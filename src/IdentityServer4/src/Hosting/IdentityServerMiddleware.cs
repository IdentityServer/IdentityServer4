// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;

namespace IdentityServer4.Hosting
{
    /// <summary>
    /// IdentityServer middleware
    /// </summary>
    public class IdentityServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="logger">The logger.</param>
        public IdentityServerMiddleware(RequestDelegate next, ILogger<IdentityServerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="router">The router.</param>
        /// <param name="session">The user session.</param>
        /// <param name="events">The event service.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context, IEndpointRouter router, IUserSession session, IEventService events)
        {
            var endpoint = context.GetEndpoint();
            var metadata = endpoint?.Metadata.GetMetadata<IdentityServerEndpointMetadata>();

            if (metadata is null)
            {
                // We only handle the request in the middleware if there's no IdentityServer endpoint resolved.'
                return Handle(context, session, events, _logger, _next, router.Find);
            }

            return _next(context);
        }

        internal static async Task Handle(HttpContext context, IUserSession session, IEventService events, ILogger logger, RequestDelegate next, Func<HttpContext, IEndpointHandler> getEndpoint)
        {
            // this will check the authentication session and from it emit the check session
            // cookie needed from JS-based signout clients.
            await session.EnsureSessionIdCookieAsync();

            try
            {
                var endpoint = getEndpoint(context);
                if (endpoint != null)
                {
                    logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());

                    var result = await endpoint.ProcessAsync(context);

                    if (result != null)
                    {
                        logger.LogTrace("Invoking result: {type}", result.GetType().FullName);

                        await result.ExecuteAsync(context);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                await events.RaiseAsync(new UnhandledExceptionEvent(ex));
                logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
                throw;
            }

            if (next != null)
            {
                await next.Invoke(context);
            }
        }
    }
}
