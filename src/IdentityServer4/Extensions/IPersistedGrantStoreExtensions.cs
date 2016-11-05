// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Extensions
{
    public static class IPersistedGrantStoreExtensions
    {
        private static PersistentGrantSerializer _serializer = new PersistentGrantSerializer();
        private const string KeySeparator = ":";

        private static string HashKey(string value, string type)
        {
            return (value + KeySeparator + type).Sha256();
        }

        private static async Task<T> GetItem<T>(this IPersistedGrantStore store, string key, string type)
        {
            key = HashKey(key, type);

            var grant = await store.GetAsync(key);
            if (grant != null && grant.Type == type)
            {
                return _serializer.Deserialize<T>(grant.Data);
            }

            return default(T);
        }

        private static async Task StoreItem<T>(this IPersistedGrantStore store, string key, T item, string type, string clientId, string subjectId, DateTime created, int lifetime)
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

            await store.StoreAsync(grant);
        }

        private static async Task RemoveItem(this IPersistedGrantStore store, string key, string type)
        {
            key = HashKey(key, type);
            await store.RemoveAsync(key);
        }


        public static Task StoreAuthorizationCodeAsync(this IPersistedGrantStore store, string handle, AuthorizationCode code)
        {
            return store.StoreItem(handle, code, Constants.PersistedGrantTypes.AuthorizationCode, code.ClientId, code.Subject.GetSubjectId(), code.CreationTime, code.Lifetime);
        }

        public static Task<AuthorizationCode> GetAuthorizationCodeAsync(this IPersistedGrantStore store, string code)
        {
            return store.GetItem<AuthorizationCode>(code, Constants.PersistedGrantTypes.AuthorizationCode);
        }

        public static Task RemoveAuthorizationCodeAsync(this IPersistedGrantStore store, string code)
        {
            return store.RemoveItem(code, Constants.PersistedGrantTypes.AuthorizationCode);
        }


        public static Task StoreReferenceTokenAsync(this IPersistedGrantStore store, string handle, Token token)
        {
            return store.StoreItem(handle, token, Constants.PersistedGrantTypes.ReferenceToken, token.ClientId, token.SubjectId, token.CreationTime, token.Lifetime);
        }

        public static Task<Token> GetReferenceTokenAsync(this IPersistedGrantStore store, string handle)
        {
            return store.GetItem<Token>(handle, Constants.PersistedGrantTypes.ReferenceToken);
        }

        public static Task RemoveReferenceTokenAsync(this IPersistedGrantStore store, string handle)
        {
            return store.RemoveItem(handle, Constants.PersistedGrantTypes.ReferenceToken);
        }

        public static Task RemoveReferenceTokensAsync(this IPersistedGrantStore store, string subjectId, string clientId)
        {
            return store.RemoveAllAsync(subjectId, clientId, Constants.PersistedGrantTypes.ReferenceToken);
        }


        public static Task StoreRefreshTokenAsync(this IPersistedGrantStore store, string handle, RefreshToken refreshToken)
        {
            return store.StoreItem(handle, refreshToken, Constants.PersistedGrantTypes.RefreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.CreationTime, refreshToken.Lifetime);
        }

        public static Task<RefreshToken> GetRefreshTokenAsync(this IPersistedGrantStore store, string refreshTokenHandle)
        {
            return store.GetItem<RefreshToken>(refreshTokenHandle, Constants.PersistedGrantTypes.RefreshToken);
        }

        public static Task RemoveRefreshTokenAsync(this IPersistedGrantStore store, string refreshTokenHandle)
        {
            return store.RemoveItem(refreshTokenHandle, Constants.PersistedGrantTypes.RefreshToken);
        }

        public static Task RemoveRefreshTokensAsync(this IPersistedGrantStore store, string subjectId, string clientId)
        {
            return store.RemoveAllAsync(subjectId, clientId, Constants.PersistedGrantTypes.RefreshToken);
        }


        private static string GetConsentKey(string subjectId, string clientId)
        {
            var key = subjectId + "|" + clientId;
            return key;
        }

        public static Task StoreUserConsentAsync(this IPersistedGrantStore store, Consent consent)
        {
            var key = GetConsentKey(consent.ClientId, consent.SubjectId);
            return store.StoreItem(key, consent, Constants.PersistedGrantTypes.UserConsent, consent.ClientId, consent.SubjectId, DateTimeHelper.UtcNow, Int32.MaxValue);
        }

        public static Task<Consent> GetUserConsentAsync(this IPersistedGrantStore store, string subjectId, string clientId)
        {
            var key = GetConsentKey(clientId, subjectId);
            return store.GetItem<Consent>(key, Constants.PersistedGrantTypes.UserConsent);
        }

        public static Task RemoveUserConsentAsync(this IPersistedGrantStore store, string subjectId, string clientId)
        {
            var key = GetConsentKey(clientId, subjectId);
            return store.RemoveItem(key, Constants.PersistedGrantTypes.UserConsent);
        }
    }
}