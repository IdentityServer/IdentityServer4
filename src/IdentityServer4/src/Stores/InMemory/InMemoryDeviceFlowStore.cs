// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// In-memory device flow store
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IDeviceFlowStore" />
    public class InMemoryDeviceFlowStore : IDeviceFlowStore
    {
        private readonly List<InMemoryDeviceAuthorization> _repository = new List<InMemoryDeviceAuthorization>();

        /// <inheritdoc/>
        public Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data, CancellationToken cancellation = default)
        {
            lock (_repository)
            {
                _repository.Add(new InMemoryDeviceAuthorization(deviceCode, userCode, data));
            }
            
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<DeviceCode> FindByUserCodeAsync(string userCode, CancellationToken cancellationToken = default)
        {
            DeviceCode foundDeviceCode;

            lock (_repository)
            {
                foundDeviceCode = _repository.FirstOrDefault(x => x.UserCode == userCode)?.Data;
            }

            return Task.FromResult(foundDeviceCode);
        }

        /// <inheritdoc/>
        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
        {
            DeviceCode foundDeviceCode;

            lock (_repository)
            {
                foundDeviceCode = _repository.FirstOrDefault(x => x.DeviceCode == deviceCode)?.Data;
            }

            return Task.FromResult(foundDeviceCode);
        }

        /// <inheritdoc/>
        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data, CancellationToken cancellationToken = default)
        {
            lock (_repository)
            {
                var foundData = _repository.FirstOrDefault(x => x.UserCode == userCode);

                if (foundData != null)
                {
                    foundData.Data = data;
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RemoveByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
        {
            lock (_repository)
            {
                var foundData = _repository.FirstOrDefault(x => x.DeviceCode == deviceCode);

                if (foundData != null)
                {
                    _repository.Remove(foundData);
                }
            }


            return Task.CompletedTask;
        }

        private class InMemoryDeviceAuthorization
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