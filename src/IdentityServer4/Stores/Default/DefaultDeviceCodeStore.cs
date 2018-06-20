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
        /// Gets the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task<DeviceCode> GetDeviceCodeAsync(string code)
        {
            return GetItemAsync(code);
        }

        /// <summary>
        /// Removes the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task RemoveDeviceCodeAsync(string code)
        {
            return RemoveItemAsync(code);
        }
    }
}