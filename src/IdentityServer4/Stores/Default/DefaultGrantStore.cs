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
        private readonly string _grantType;
        private readonly ILogger _logger;
        private readonly IPersistedGrantStore _store;
        private readonly IPersistentGrantSerializer _serializer;
        private readonly IHandleGenerationService _handleGenerationService;

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

            _grantType = grantType;
            _store = store;
            _serializer = serializer;
            _handleGenerationService = handleGenerationService;
            _logger = logger;
        }

        const string KeySeparator = ":";

        /// <summary>
        /// Gets the hashed key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected string GetHashedKey(string value)
        {
            return (value + KeySeparator + _grantType).Sha256();
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected async Task<T> GetItemAsync(string key)
        {
            var hashedKey = GetHashedKey(key);

            var grant = await _store.GetAsync(hashedKey);
            if (grant != null && grant.Type == _grantType)
            {
                if (!grant.Expiration.HasExpired())
                {
                    try
                    {
                        return _serializer.Deserialize<T>(grant.Data);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to deserailize JSON from grant store. Exception: {0}", ex.Message);
                    }
                }
                else
                {
                    _logger.LogDebug("{grantType} grant with value: {key} found in store, but has expired.", _grantType, key);
                }
            }
            else
            {
                _logger.LogDebug("{grantType} grant with value: {key} not found in store.", _grantType, key);
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
        protected async Task<string> CreateItemAsync(T item, string clientId, string subjectId, DateTime created, int lifetime)
        {
            var handle = await _handleGenerationService.GenerateAsync();
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
        protected Task StoreItemAsync(string key, T item, string clientId, string subjectId, DateTime created, int lifetime)
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
        protected async Task StoreItemAsync(string key, T item, string clientId, string subjectId, DateTime created, DateTime? expiration)
        {
            key = GetHashedKey(key);

            var json = _serializer.Serialize(item);

            var grant = new PersistedGrant
            {
                Key = key,
                Type = _grantType,
                ClientId = clientId,
                SubjectId = subjectId,
                CreationTime = created,
                Expiration = expiration,
                Data = json
            };

            await _store.StoreAsync(grant);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected async Task RemoveItemAsync(string key)
        {
            key = GetHashedKey(key);
            await _store.RemoveAsync(key);
        }

        /// <summary>
        /// Removes all items for a subject id / cliend id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        protected async Task RemoveAllAsync(string subjectId, string clientId)
        {
            await _store.RemoveAllAsync(subjectId, clientId, _grantType);
        }
    }
}