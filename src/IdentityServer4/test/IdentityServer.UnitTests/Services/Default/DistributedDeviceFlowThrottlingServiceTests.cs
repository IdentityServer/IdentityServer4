using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class DistributedDeviceFlowThrottlingServiceTests
    {
        private TestCache cache = new TestCache();

        private readonly IdentityServerOptions options = new IdentityServerOptions {DeviceFlow = new DeviceFlowOptions {Interval = 5}};
        private readonly DeviceCode deviceCode = new DeviceCode
        {
            Lifetime = 300,
            CreationTime = DateTime.UtcNow
        };

        private const string CacheKey = "devicecode_";
        private readonly DateTime testDate = new DateTime(2018, 06, 28, 13, 37, 42);

        [Fact]
        public async Task First_Poll()
        {
            var handle = Guid.NewGuid().ToString();
            var service = new DistributedDeviceFlowThrottlingService(cache, new StubClock {UtcNowFunc = () => testDate}, options);

            var result = await service.ShouldSlowDown(handle, deviceCode);

            result.Should().BeFalse();

            CheckCacheEntry(handle);
        }

        [Fact]
        public async Task Second_Poll_Too_Fast()
        {
            var handle = Guid.NewGuid().ToString();
            var service = new DistributedDeviceFlowThrottlingService(cache, new StubClock { UtcNowFunc = () => testDate }, options);

            cache.Set(CacheKey + handle, Encoding.UTF8.GetBytes(testDate.AddSeconds(-1).ToString("O")));

            var result = await service.ShouldSlowDown(handle, deviceCode);

            result.Should().BeTrue();
            
            CheckCacheEntry(handle);
        }

        [Fact]
        public async Task Second_Poll_After_Interval()
        {
            var handle = Guid.NewGuid().ToString();
            
            var service = new DistributedDeviceFlowThrottlingService(cache, new StubClock { UtcNowFunc = () => testDate }, options);

            cache.Set($"devicecode_{handle}", Encoding.UTF8.GetBytes(testDate.AddSeconds(-deviceCode.Lifetime - 1).ToString("O")));

            var result = await service.ShouldSlowDown(handle, deviceCode);

            result.Should().BeFalse();

            CheckCacheEntry(handle);
        }

        private void CheckCacheEntry(string handle)
        {
            cache.Items.TryGetValue(CacheKey + handle, out var values).Should().BeTrue();

            var dateTimeAsString = Encoding.UTF8.GetString(values?.Item1);
            var dateTime = DateTime.Parse(dateTimeAsString);
            dateTime.Should().Be(testDate);

            values?.Item2.AbsoluteExpiration.Should().BeCloseTo(deviceCode.CreationTime.AddSeconds(deviceCode.Lifetime));
        }
    }

    internal class TestCache : IDistributedCache
    {
        public readonly Dictionary<string, Tuple<byte[], DistributedCacheEntryOptions>> Items =
            new Dictionary<string, Tuple<byte[], DistributedCacheEntryOptions>>();

        public byte[] Get(string key)
        {
            if (Items.TryGetValue(key, out var value)) return value.Item1;
            return null;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Items.Remove(key);

            Items.Add(key, new Tuple<byte[], DistributedCacheEntryOptions>(value, options));
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}