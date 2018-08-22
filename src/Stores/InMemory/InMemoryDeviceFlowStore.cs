// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// In-memory device flow store
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IDeviceFlowStore" />
    public class InMemoryDeviceFlowStore : IDeviceFlowStore
    {
        private readonly IHandleGenerationService _handleGenerationService;
        private readonly List<InMemoryDeviceAuthorization> _repository = new List<InMemoryDeviceAuthorization>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDeviceFlowStore"/> class.
        /// </summary>
        /// <param name="handleGenerationService">The handle generation service.</param>
        public InMemoryDeviceFlowStore(IHandleGenerationService handleGenerationService)
        {
            _handleGenerationService = handleGenerationService;
        }

        /// <summary>
        /// Stores the device authorization request.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data)
        {
            var deviceCode = await _handleGenerationService.GenerateAsync();
            _repository.Add(new InMemoryDeviceAuthorization(deviceCode, userCode, data));
            return deviceCode;
        }

        /// <summary>
        /// Finds device authorization by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        public Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            return Task.FromResult(_repository.FirstOrDefault(x => x.UserCode == userCode)?.Data);
        }

        /// <summary>
        /// Finds device authorization by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            return Task.FromResult(_repository.FirstOrDefault(x => x.DeviceCode == deviceCode)?.Data);
        }

        /// <summary>
        /// Updates device authorization, searching by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            var foundData = _repository.FirstOrDefault(x => x.UserCode == userCode);

            if (foundData != null)
            {
                foundData.Data = data;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the device authorization, searching by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            var foundData = _repository.FirstOrDefault(x => x.DeviceCode == deviceCode);

            if (foundData != null)
            {
                _repository.Remove(foundData);
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