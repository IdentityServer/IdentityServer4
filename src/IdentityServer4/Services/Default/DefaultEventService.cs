/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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