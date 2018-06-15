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
    /// Default user code store.
    /// </summary>
    public class DefaultUserCodeStore : DefaultGrantStore<UserCode>, IUserCodeStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserCodeStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        protected DefaultUserCodeStore(
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger logger)
            : base(IdentityServerConstants.PersistedGrantTypes.DeviceCode, store, serializer, handleGenerationService, logger)
        {
        }

        /// <summary>
        /// Stores the user code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Task StoreUserCodeAsync(string code, UserCode data)
        {
            return StoreItemAsync(code, data, data.ClientId, null, data.CreationTime, data.Lifetime);
        }

        /// <summary>
        /// Gets the user code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task<UserCode> GetUserCodeAsync(string code)
        {
            return GetItemAsync(code);
        }

        /// <summary>
        /// Removes the user code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task RemoveUserCodeAsync(string code)
        {
            return RemoveItemAsync(code);
        }
    }
}