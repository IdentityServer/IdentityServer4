// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bornlogic.IdentityServer.Events;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Hosting
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
        /// <param name="backChannelLogoutService"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IEndpointRouter router, IUserSession session, IEventService events, IBackChannelLogoutService backChannelLogoutService)
        {
            // this will check the authentication session and from it emit the check session
            // cookie needed from JS-based signout clients.
            await session.EnsureSessionIdCookieAsync();

            context.Response.OnStarting(async () =>
            {
                if (context.GetSignOutCalled())
                {
                    _logger.LogDebug("SignOutCalled set; processing post-signout session cleanup.");

                    // this clears our session id cookie so JS clients can detect the user has signed out
                    await session.RemoveSessionIdCookieAsync();

                    // back channel logout
                    var logoutContext = await session.GetLogoutNotificationContext();
                    if (logoutContext != null)
                    {
                        await backChannelLogoutService.SendLogoutNotificationsAsync(logoutContext);
                    }
                }
            });

            try
            {
                var endpoint = router.Find(context);
                if (endpoint != null)
                {
                    _logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());

                    var result = await endpoint.ProcessAsync(context);

                    if (result != null)
                    {
                        _logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                        await result.ExecuteAsync(context);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                await events.RaiseAsync(new UnhandledExceptionEvent(ex));
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
                throw;
            }

            await _next(context);
        }
    }
}
