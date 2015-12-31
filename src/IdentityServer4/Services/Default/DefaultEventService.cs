// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Events;
using IdentityServer4.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.Default
{
    /// <summary>
    /// Default implementation of the event service. Write events raised to the log.
    /// </summary>
    public class DefaultEventService : IEventService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;

        public DefaultEventService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DefaultEventService>();
        }

        /// <summary>
        /// Raises the specified event.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <exception cref="System.ArgumentNullException">evt</exception>
        public virtual Task RaiseAsync<T>(Event<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");
            
            var json = LogSerializer.Serialize(evt);
            _logger.LogInformation(json);

            return Task.FromResult(0);
        }
    }
}