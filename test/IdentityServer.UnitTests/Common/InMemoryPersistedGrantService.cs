// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace IdentityServer4.UnitTests.Common
{
    public class InMemoryPersistedGrantService : IPersistedGrantService
    {
        Dictionary<string, AuthorizationCode> _codes = new Dictionary<string, AuthorizationCode>();
        Dictionary<string, Token> _referenceTokens = new Dictionary<string, Token>();
        Dictionary<string, RefreshToken> _refreshTokens = new Dictionary<string, RefreshToken>();
        Dictionary<string, Consent> _userConsent = new Dictionary<string, Consent>();

        public Task<AuthorizationCode> GetAuthorizationCodeAsync(string code)
        {
            if (_codes.ContainsKey(code))
            {
                return Task.FromResult(_codes[code]);
            }
            return Task.FromResult<AuthorizationCode>(null);
        }

        public Task<Token> GetReferenceTokenAsync(string handle)
        {
            if (_referenceTokens.ContainsKey(handle))
            {
                return Task.FromResult(_referenceTokens[handle]);
            }
            return Task.FromResult<Token>(null);
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle)
        {
            if (_refreshTokens.ContainsKey(refreshTokenHandle))
            {
                return Task.FromResult(_refreshTokens[refreshTokenHandle]);
            }
            return Task.FromResult<RefreshToken>(null);
        }

        public Task RemoveAuthorizationCodeAsync(string code)
        {
            _codes.Remove(code);
            return Task.FromResult(0);
        }

        public Task RemoveReferenceTokenAsync(string handle)
        {
            _referenceTokens.Remove(handle);
            return Task.FromResult(0);
        }

        public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
        {
            var keys = _referenceTokens.Where(x => x.Value.SubjectId == subjectId && x.Value.ClientId == clientId).Select(x=>x.Key);
            foreach(var key in keys.ToArray())
            {
                _referenceTokens.Remove(key);
            }
            return Task.FromResult(0);
        }

        public Task RemoveRefreshTokenAsync(string refreshTokenHandle)
        {
            _refreshTokens.Remove(refreshTokenHandle);
            return Task.FromResult(0);
        }

        public Task RemoveRefreshTokensAsync(string subjectId, string clientId)
        {
            var keys = _refreshTokens.Where(x => x.Value.SubjectId == subjectId && x.Value.ClientId == clientId).Select(x => x.Key);
            foreach (var key in keys.ToArray())
            {
                _refreshTokens.Remove(key);
            }
            return Task.FromResult(0);
        }

        public Task StoreAuthorizationCodeAsync(string id, AuthorizationCode code)
        {
            _codes[id] = code;
            return Task.FromResult(0);
        }

        public Task StoreReferenceTokenAsync(string handle, Token token)
        {
            _referenceTokens[handle] = token;
            return Task.FromResult(0);
        }

        public Task StoreRefreshTokenAsync(string handle, RefreshToken refreshToken)
        {
            _refreshTokens[handle] = refreshToken;
            return Task.FromResult(0);
        }

        string GetConsentKey(string subjectId, string clientId)
        {
            return subjectId + "|" + clientId;
        }

        public Task RemoveUserConsent(string subjectId, string clientId)
        {
            var key = GetConsentKey(subjectId, clientId);
            if(_userConsent.ContainsKey(key))
            {
                _userConsent.Remove(key);
            }
            return Task.FromResult(0);
        }
        public Task StoreUserConsent(Consent consent)
        {
            var key = GetConsentKey(consent.SubjectId, consent.ClientId);
            _userConsent[key] = consent;
            return Task.FromResult(0);
        }
        public Task<Consent> GetUserConsent(string subjectId, string clientId)
        {
            Consent result = null;
            var key = GetConsentKey(subjectId, clientId);
            if (_userConsent.ContainsKey(key))
            {
                result = _userConsent[key];
            }
            return Task.FromResult(result);
        }
    }
}