// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Base class for persisting grants using the IPersistedGrantStore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultGrantStore<T>
    {
        /// <summary>
        /// The grant type being stored.
        /// </summary>
        protected string GrantType { get; }

        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// The PersistedGrantStore.
        /// </summary>
        protected IPersistedGrantStore Store { get; }

        /// <summary>
        /// The PersistentGrantSerializer;
        /// </summary>
        protected IPersistentGrantSerializer Serializer { get; }

        /// <summary>
        /// The HandleGenerationService.
        /// </summary>
        protected IHandleGenerationService HandleGenerationService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGrantStore{T}"/> class.
        /// </summary>
        /// <param name="grantType">Type of the grant.</param>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">grantType</exception>
        protected DefaultGrantStore(string grantType,
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger logger)
        {
            if (grantType.IsMissing()) throw new ArgumentNullException(nameof(grantType));

            GrantType = grantType;
            Store = store;
            Serializer = serializer;
            HandleGenerationService = handleGenerationService;
            Logger = logger;
        }

        private const string KeySeparator = ":";

        /// <summary>
        /// Gets the hashed key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected virtual string GetHashedKey(string value)
        {
            return (value + KeySeparator + GrantType).Sha256();
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected virtual async Task<T> GetItemAsync(string key)
        {
            var hashedKey = GetHashedKey(key);

            var grant = await Store.GetAsync(hashedKey);
            if (grant != null && grant.Type == GrantType)
            {
                try
                {
                    return Serializer.Deserialize<T>(grant.Data);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to deserialize JSON from grant store.");
                }
            }
            else
            {
                Logger.LogDebug("{grantType} grant with value: {key} not found in store.", GrantType, key);
            }

            return default(T);
        }

        /// <summary>
        /// Creates the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="created">The created.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <returns></returns>
        protected virtual async Task<string> CreateItemAsync(T item, string clientId, string subjectId, DateTime created, int lifetime)
        {
            var handle = await HandleGenerationService.GenerateAsync();
            await StoreItemAsync(handle, item, clientId, subjectId, created, created.AddSeconds(lifetime));
            return handle;
        }

        /// <summary>
        /// Stores the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="created">The created.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <returns></returns>
        protected virtual Task StoreItemAsync(string key, T item, string clientId, string subjectId, DateTime created, int lifetime)
        {
            return StoreItemAsync(key, item, clientId, subjectId, created, created.AddSeconds(lifetime));
        }

        /// <summary>
        /// Stores the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="created">The created.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        protected virtual async Task StoreItemAsync(string key, T item, string clientId, string subjectId, DateTime created, DateTime? expiration)
        {
            key = GetHashedKey(key);

            var json = Serializer.Serialize(item);

            var grant = new PersistedGrant
            {
                Key = key,
                Type = GrantType,
                ClientId = clientId,
                SubjectId = subjectId,
                CreationTime = created,
                Expiration = expiration,
                Data = json
            };

            await Store.StoreAsync(grant);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected virtual async Task RemoveItemAsync(string key)
        {
            key = GetHashedKey(key);
            await Store.RemoveAsync(key);
        }

        /// <summary>
        /// Removes all items for a subject id / cliend id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        protected virtual async Task RemoveAllAsync(string subjectId, string clientId)
        {
            await Store.RemoveAllAsync(subjectId, clientId, GrantType);
        }
    }
}