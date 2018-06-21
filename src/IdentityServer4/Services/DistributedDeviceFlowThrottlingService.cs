using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;

namespace IdentityServer4.Services
{
    public class DistributedDeviceFlowThrottlingService : IDeviceFlowThrottlingService
    {
        private readonly IDistributedCache _cache;
        private readonly ISystemClock _clock;
        private readonly IdentityServerOptions _options;

        private const string _keyPrefix = "devicecode_";

        public DistributedDeviceFlowThrottlingService(
            IDistributedCache cache,
            ISystemClock clock,
            IdentityServerOptions options)
        {
            _cache = cache;
            _clock = clock;
            _options = options;
        }

        public async Task<bool> ShouldSlowDown(string deviceCode, DeviceCode details)
        {
            if (deviceCode == null) throw new ArgumentNullException(nameof(deviceCode));
            
            var key = _keyPrefix + deviceCode;
            var options = new DistributedCacheEntryOptions {AbsoluteExpiration = details.CreationTime.AddSeconds(details.Lifetime)};

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
                if (lastSeen.AddSeconds(_options.DeviceFlow.Interval) < _clock.UtcNow)
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