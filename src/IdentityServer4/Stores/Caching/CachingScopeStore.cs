// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Stores
{
    public class CachingScopeStore<T> : IScopeStore
        where T : IScopeStore
    {
        const string AllScopes = "__all_scopes__";
        const string AllScopesPublic = "__public_only__";

        private readonly IdentityServerOptions _options;
        private readonly ICache<IEnumerable<Scope>> _cache;
        private readonly IScopeStore _inner;
        private readonly ILogger<CachingScopeStore<T>> _logger;

        public CachingScopeStore(IdentityServerOptions options, T inner, ICache<IEnumerable<Scope>> cache, ILogger<CachingScopeStore<T>> logger)
        {
            _options = options;
            _inner = inner;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            var key = GetKey(scopeNames);

            var scopes = await _cache.GetAsync(key,
                _options.CachingOptions.ScopeStoreExpiration,
                () => _inner.FindScopesAsync(scopeNames),
                _logger);

            return scopes;
        }

        public async Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var key = GetKey(publicOnly);

            var scopes = await _cache.GetAsync(key,
                _options.CachingOptions.ScopeStoreExpiration,
                () => _inner.GetScopesAsync(publicOnly),
                _logger);

            return scopes;
        }

        private string GetKey(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null || !scopeNames.Any()) return "";
            return scopeNames.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
        }

        private string GetKey(bool publicOnly)
        {
            if (publicOnly) return AllScopesPublic;
            return AllScopes;
        }
    }
}