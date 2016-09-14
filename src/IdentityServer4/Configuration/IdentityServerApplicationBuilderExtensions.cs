// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Hosting;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class IdentityServerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder app)
        {
            app.Validate();

            app.UseMiddleware<BaseUrlMiddleware>();

            app.ConfigureCors();
            app.ConfigureCookies();

            app.UseMiddleware<IdentityServerMiddleware>();

            return app;
        }

        internal static void Validate(this IApplicationBuilder app)
        {
            var loggerFactory = app.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory.CreateLogger("IdentityServer4.Startup");

            // todo: which other services to test for?
            app.TestService(typeof(IPersistedGrantStore), logger, "No storage mechanism for grants specified. Use the 'AddInMemoryStores' extension method to register a development version.");
        }

        internal static void TestService(this IApplicationBuilder app, Type service, ILogger logger, string message = null)
        {
            var appService = app.ApplicationServices.GetService(service);

            if (appService == null)
            {
                var error = message ?? $"Required service {service.GetType().FullName} is not registered in the DI container. Aborting startup";

                logger.LogCritical(error);
                throw new InvalidOperationException(error);
            }
        }
    }
}