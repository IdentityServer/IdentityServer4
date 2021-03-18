using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Stores.Default;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace IdentityServer.UnitTests.Stores.Default
{
    public class DistributedCacheAuthorizationParametersMessageStoreTests
    {
        [Fact]
        public async Task DeleteAsync_should_prefix_id_with_CacheKeyPrefix()
        {
            var distributedCache = new MockDistributedCache();

            var messageStore = new DistributedCacheAuthorizationParametersMessageStore(distributedCache, null);
            await messageStore.DeleteAsync("id");

            distributedCache.LastKeyRemoveRequest.Should().Be("DistributedCacheAuthorizationParametersMessageStore-id");
        }

        private class MockDistributedCache : IDistributedCache
        {
            public string LastKeyRemoveRequest { get; private set; }

            public byte[] Get(string key) => throw new NotImplementedException();

            public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
                => throw new NotImplementedException();

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
                => throw new NotImplementedException();

            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
                => throw new NotImplementedException();

            public void Refresh(string key) => throw new NotImplementedException();

            public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
                => throw new NotImplementedException();

            public void Remove(string key) => throw new NotImplementedException();

            public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
            {
                LastKeyRemoveRequest = key;
                return Task.CompletedTask;
            }
        }
    }
}
