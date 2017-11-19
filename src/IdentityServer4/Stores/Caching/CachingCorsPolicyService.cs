// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Services;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Caching decorator for ICorsPolicyService
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.ICorsPolicyService" />
    public class CachingCorsPolicyService<T> : ICorsPolicyService
        where T : ICorsPolicyService
    {
        /// <summary>
        /// Class to model entries in CORS origin cache.
        /// </summary>
        public class CorsCacheEntry
        {
            /// <summary>
            /// Ctor.
            /// </summary>
            public CorsCacheEntry(bool allowed)
            {
                Allowed = allowed;
            }

            /// <summary>
            /// Is origin allowed.
            /// </summary>
            public bool Allowed { get; }
        }

        private readonly IdentityServerOptions _options;
        private readonly ICache<CorsCacheEntry> _corsCache;
        private readonly ICorsPolicyService _inner;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingResourceStore{T}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="corsCache">The CORS origin cache.</param>
        /// <param name="logger">The logger.</param>
        public CachingCorsPolicyService(IdentityServerOptions options, 
            T inner, 
            ICache<CorsCacheEntry> corsCache, 
            ILogger<CachingCorsPolicyService<T>> logger)
        {
            _options = options;
            _inner = inner;
            _corsCache = corsCache;
            _logger = logger;
        }

        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public async Task<bool> IsOriginAllowedAsync(string origin)
        {
            var entry = await _corsCache.GetAsync(origin,
                          _options.Caching.CorsExpiration,
                          async () => new CorsCacheEntry(await _inner.IsOriginAllowedAsync(origin)),
                          _logger);

            return entry.Allowed;
        }
    }
}