// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Services.Default
{
    /// <summary>
    /// Default persisted grant service
    /// </summary>
    public class DefaultPersistedGrantService : IPersistedGrantService
    {
        private readonly ILogger<DefaultPersistedGrantService> _logger;
        private readonly IPersistedGrantStore _store;
        private readonly PersistentGrantSerializer _serializer;

        public DefaultPersistedGrantService(IPersistedGrantStore store, 
            PersistentGrantSerializer serializer,
            ILogger<DefaultPersistedGrantService> logger)
        {
            _store = store;
            _serializer = serializer;
            _logger = logger;
        }

        const string KeySeparator = ":";

        string HashKey(string value, string type)
        {
            return (value + KeySeparator + type).Sha256();
        }

        async Task<T> GetItem<T>(string key, string type)
        {
            key = HashKey(key, type);

            var grant = await _store.GetAsync(key);
            if (grant != null && grant.Type == type)
            {
                return _serializer.Deserialize<T>(grant.Data);
            }

            return default(T);
        }

        async Task StoreItem<T>(string key, T item, string type, string clientId, string subjectId, DateTime created, int lifetime)
        {
            key = HashKey(key, type);

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

        async Task RemoveItem(string key, string type)
        {
            key = HashKey(key, type);
            await _store.RemoveAsync(key);
        }


        public Task StoreAuthorizationCodeAsync(string handle, AuthorizationCode code)
        {
            return StoreItem(handle, code, Constants.PersistedGrantTypes.AuthorizationCode, code.ClientId, code.Subject.GetSubjectId(), code.CreationTime, code.Lifetime);
        }

        public Task<AuthorizationCode> GetAuthorizationCodeAsync(string code)
        {
            return GetItem<AuthorizationCode>(code, Constants.PersistedGrantTypes.AuthorizationCode);
        }

        public Task RemoveAuthorizationCodeAsync(string code)
        {
            return RemoveItem(code, Constants.PersistedGrantTypes.AuthorizationCode);
        }


        public Task StoreReferenceTokenAsync(string handle, Token token)
        {
            return StoreItem(handle, token, Constants.PersistedGrantTypes.ReferenceToken, token.ClientId, token.SubjectId, token.CreationTime, token.Lifetime);
        }

        public Task<Token> GetReferenceTokenAsync(string handle)
        {
            return GetItem<Token>(handle, Constants.PersistedGrantTypes.ReferenceToken);
        }

        public Task RemoveReferenceTokenAsync(string handle)
        {
            return RemoveItem(handle, Constants.PersistedGrantTypes.ReferenceToken);
        }

        public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
        {
            return _store.RemoveAllAsync(subjectId, clientId, Constants.PersistedGrantTypes.ReferenceToken);
        }


        public Task StoreRefreshTokenAsync(string handle, RefreshToken refreshToken)
        {
            return StoreItem(handle, refreshToken, Constants.PersistedGrantTypes.RefreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.CreationTime, refreshToken.Lifetime);
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle)
        {
            return GetItem<RefreshToken>(refreshTokenHandle, Constants.PersistedGrantTypes.RefreshToken);
        }

        public Task RemoveRefreshTokenAsync(string refreshTokenHandle)
        {
            return RemoveItem(refreshTokenHandle, Constants.PersistedGrantTypes.RefreshToken);
        }
        
        public Task RemoveRefreshTokensAsync(string subjectId, string clientId)
        {
            return _store.RemoveAllAsync(subjectId, clientId, Constants.PersistedGrantTypes.RefreshToken);
        }


        string GetConsentKey(string subjectId, string clientId)
        {
            var key = subjectId + "|" + clientId;
            return key;
        }

        public Task StoreUserConsentAsync(Consent consent)
        {
            var key = GetConsentKey(consent.ClientId, consent.SubjectId);
            return StoreItem(key, consent, Constants.PersistedGrantTypes.UserConsent, consent.ClientId, consent.SubjectId, DateTimeHelper.UtcNow, Int32.MaxValue);
        }

        public Task<Consent> GetUserConsentAsync(string subjectId, string clientId)
        {
            var key = GetConsentKey(clientId, subjectId);
            return GetItem<Consent>(key, Constants.PersistedGrantTypes.UserConsent);
        }

        public Task RemoveUserConsentAsync(string subjectId, string clientId)
        {
            var key = GetConsentKey(clientId, subjectId);
            return RemoveItem(key, Constants.PersistedGrantTypes.UserConsent);
        }

        public async Task<IEnumerable<Consent>> GetAllGrantsAsync(string subjectId)
        {
            var grants = await _store.GetAllAsync(subjectId);

            var consents = grants.Where(x => x.Type == Constants.PersistedGrantTypes.UserConsent)
                .Select(x => _serializer.Deserialize<Consent>(x.Data));

            var codes = grants.Where(x => x.Type == Constants.PersistedGrantTypes.AuthorizationCode)
                .Select(x => _serializer.Deserialize<AuthorizationCode>(x.Data))
                .Select(x => new Consent
                {
                    ClientId = x.ClientId,
                    SubjectId = subjectId,
                    Scopes = x.Scopes
                });

            var refresh = grants.Where(x => x.Type == Constants.PersistedGrantTypes.RefreshToken)
                .Select(x => _serializer.Deserialize<RefreshToken>(x.Data))
                .Select(x => new Consent
                {
                    ClientId = x.ClientId,
                    SubjectId = subjectId,
                    Scopes = x.Scopes
                });

            var access = grants.Where(x => x.Type == Constants.PersistedGrantTypes.ReferenceToken)
                .Select(x => _serializer.Deserialize<Token>(x.Data))
                .Select(x => new Consent
                {
                    ClientId = x.ClientId,
                    SubjectId = subjectId,
                    Scopes = x.Scopes
                });

            consents = Join(consents, codes);
            consents = Join(consents, refresh);
            consents = Join(consents, access);

            return consents.ToArray();
        }

        IEnumerable<Consent> Join(IEnumerable<Consent> first, IEnumerable<Consent> second)
        {
            var query =
                from f in first
                join s in second on f.ClientId equals s.ClientId
                let scopes = f.Scopes.Union(s.Scopes).Distinct()
                select new Consent
                {
                    ClientId = f.ClientId,
                    SubjectId = f.SubjectId,
                    Scopes = scopes
                };
            return query;
        }

        public Task RemoveAllGrantsAsync(string subjectId, string clientId)
        {
            return _store.RemoveAllAsync(subjectId, clientId);
        }
    }
}