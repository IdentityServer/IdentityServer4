// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;

namespace IdentityServer4.Services
{
    /// <summary>
    /// The default device flow throttling service using IDistributedCache.
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IDeviceFlowThrottlingService" />
    public class DistributedDeviceFlowThrottlingService : IDeviceFlowThrottlingService
    {
        private readonly IDistributedCache _cache;
        private readonly ISystemClock _clock;
        private readonly IdentityServerOptions _options;

        private const string _keyPrefix = "devicecode_";

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedDeviceFlowThrottlingService"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="options">The options.</param>
        public DistributedDeviceFlowThrottlingService(
            IDistributedCache cache,
            ISystemClock clock,
            IdentityServerOptions options)
        {
            _cache = cache;
            _clock = clock;
            _options = options;
        }

        /// <summary>
        /// Decides if the requesting client and device code needs to slow down.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="details">The device code details.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">deviceCode</exception>
        public async Task<bool> ShouldSlowDown(string deviceCode, DeviceCode details)
        {
            if (deviceCode == null) throw new ArgumentNullException(nameof(deviceCode));
            
            var key = _keyPrefix + deviceCode;
            var options = new DistributedCacheEntryOptions {AbsoluteExpiration = _clock.UtcNow.AddSeconds(details.Lifetime)};

            var lastSeenAsString = await _cache.GetStringAsync(key);

            // record new
            if (lastSeenAsString == null)
            {
                await _cache.SetStringAsync(key, _clock.UtcNow.ToString("O"), options);
                return false;
            }

            // check interval
            if (DateTime.TryParse(lastSeenAsString, out var lastSeen))
            {
                if (_clock.UtcNow < lastSeen.AddSeconds(_options.DeviceFlow.Interval))
                {
                    await _cache.SetStringAsync(key, _clock.UtcNow.ToString("O"), options);
                    return true;
                }
            }

            // store current and continue
            await _cache.SetStringAsync(key, _clock.UtcNow.ToString("O"), options);
            return false;
        }
    }
}