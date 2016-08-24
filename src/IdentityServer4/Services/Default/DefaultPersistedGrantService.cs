// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;

namespace IdentityServer4.Services.Default
{
    /// <summary>
    /// Default persisted grant service
    /// </summary>
    public class DefaultPersistedGrantService : IPersistedGrantService
    {
        private readonly IClientStore _clientStore;
        private readonly ILogger<DefaultPersistedGrantService> _logger;
        private readonly IPersistedGrantStore _store;
        private readonly PersistentGrantSerializer _serializer;

        public DefaultPersistedGrantService(IPersistedGrantStore store, 
            IClientStore clientStore,
            PersistentGrantSerializer serializer,
            ILogger<DefaultPersistedGrantService> logger)
        {
            _store = store;
            _clientStore = clientStore;
            _serializer = serializer;
            _logger = logger;
        }

        string HashKey(string value)
        {
            return value.Sha256();
        }

        async Task<T> GetItem<T>(string key, string type)
        {
            key = HashKey(key);

            var grant = await _store.GetAsync(key);
            if (grant != null && grant.Type == type)
            {
                return _serializer.Deserialize<T>(grant.Data);
            }

            return default(T);
        }

        async Task StoreItem<T>(string key, T item, string type, string clientId, string subjectId, DateTime created, int lifetime)
        {
            key = HashKey(key);

            var json = _serializer.Serialize(item);

            var grant = new PersistedGrant()
            {
                Key = key,
                Type = type,
                ClientId = clientId,
                SubjectId = subjectId,
                CreationTime = created,
                Expiration = created.AddSeconds(lifetime),
                Data = json,
            };

            await _store.StoreAsync(grant);
        }

        async Task RemoveItem(string key)
        {
            key = HashKey(key);
            await _store.RemoveAsync(key);
        }

        public Task<AuthorizationCode> GetAuthorizationCodeAsync(string code)
        {
            return GetItem<AuthorizationCode>(code, Constants.PersistedGrantTypes.AuthorizationCode);
        }

        public Task<Token> GetReferenceTokenAsync(string handle)
        {
            return GetItem<Token>(handle, Constants.PersistedGrantTypes.ReferenceToken);
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle)
        {
            return GetItem<RefreshToken>(refreshTokenHandle, Constants.PersistedGrantTypes.RefreshToken);
        }

        public Task RemoveAuthorizationCodeAsync(string code)
        {
            return RemoveItem(code);
        }

        public Task RemoveReferenceTokenAsync(string handle)
        {
            return RemoveItem(handle);
        }

        public Task RemoveRefreshTokenAsync(string refreshTokenHandle)
        {
            return RemoveItem(refreshTokenHandle);
        }

        public Task StoreRefreshTokenAsync(string handle, RefreshToken refreshToken)
        {
            return StoreItem(handle, refreshToken, Constants.PersistedGrantTypes.RefreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.CreationTime.UtcDateTime, refreshToken.Lifetime);
        }

        public Task StoreAuthorizationCodeAsync(string id, AuthorizationCode code)
        {
            return StoreItem(id, code, Constants.PersistedGrantTypes.AuthorizationCode, code.ClientId, code.Subject.GetSubjectId(), code.CreationTime.UtcDateTime, code.Lifetime);
        }

        public Task StoreReferenceTokenAsync(string handle, Token token)
        {
            return StoreItem(handle, token, Constants.PersistedGrantTypes.ReferenceToken, token.ClientId, token.SubjectId, token.CreationTime.UtcDateTime, token.Lifetime);
        }

        public Task RemoveRefreshTokensAsync(string subjectId, string clientId)
        {
            return _store.RemoveAsync(subjectId, clientId, Constants.PersistedGrantTypes.RefreshToken);
        }

        public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
        {
            return _store.RemoveAsync(subjectId, clientId, Constants.PersistedGrantTypes.ReferenceToken);
        }
    }
}