// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Default device code store.
    /// </summary>
    public class DefaultDeviceCodeStore : DefaultGrantStore<DeviceCode>, IDeviceCodeStore
    {
        private const string AuthorizedPrefix = "COMPLETED_";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDeviceCodeStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultDeviceCodeStore(
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger<DefaultDeviceCodeStore> logger)
            : base(IdentityServerConstants.PersistedGrantTypes.DeviceCode, store, serializer, handleGenerationService, logger)
        {
        }

        /// <summary>
        /// Stores the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task<string> StoreDeviceCodeAsync(DeviceCode code)
        {
            return CreateItemAsync(code, code.ClientId, null, code.CreationTime, code.Lifetime);
        }

        /// <summary>
        /// Stores the authorized device code asynchronous.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Task StoreAuthorizedDeviceCodeAsync(string code, DeviceCode data)
        {
            return StoreItemAsync(AuthorizedCode(code), data, data.ClientId, null, data.CreationTime, data.Lifetime);
        }

        /// <summary>
        /// Gets the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public async Task<DeviceCode> GetDeviceCodeAsync(string code)
        {
            return await GetItemAsync(AuthorizedCode(code))
                   ?? await GetItemAsync(code);
        }

        /// <summary>
        /// Removes the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public async Task RemoveDeviceCodeAsync(string code)
        {
            await RemoveItemAsync(code);
            await RemoveItemAsync(AuthorizedCode(code));
        }

        private string AuthorizedCode(string code) => AuthorizedPrefix + code;
    }
}