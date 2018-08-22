using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common
{
    public class MockDeviceFlowStore : IDeviceFlowStore
    {
        public readonly List<InMemoryDeviceAuthorization> Data = new List<InMemoryDeviceAuthorization>();

        public Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data)
        {
            var deviceCode = Guid.NewGuid().ToString();

            Data.Add(new InMemoryDeviceAuthorization(deviceCode, userCode, data));
            return Task.FromResult(deviceCode);
        }

        public Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.UserCode == userCode)?.Data);
        }

        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.DeviceCode == deviceCode)?.Data);
        }

        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            var foundData = Data.FirstOrDefault(x => x.UserCode == userCode);

            if (foundData != null)
            {
                foundData.Data = data;
            }

            return Task.CompletedTask;
        }

        public Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            var foundData = Data.FirstOrDefault(x => x.DeviceCode == deviceCode);

            if (foundData != null)
            {
                Data.Remove(foundData);
            }

            return Task.CompletedTask;
        }

        public class InMemoryDeviceAuthorization
        {
            public InMemoryDeviceAuthorization(string deviceCode, string userCode, DeviceCode data)
            {
                DeviceCode = deviceCode;
                UserCode = userCode;
                Data = data;
            }

            public string DeviceCode { get; }
            public string UserCode { get; }
            public DeviceCode Data { get; set; }
        }
    }
}