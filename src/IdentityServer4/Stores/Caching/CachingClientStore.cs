// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Threading.Tasks;
using System;
using IdentityServer4.Configuration;

namespace IdentityServer4.Stores
{
    public class CachingClientStore<T> : IClientStore
        where T : IClientStore
    {
        private readonly IdentityServerOptions _options;
        private readonly ICache<Client> _cache;
        private readonly IClientStore _inner;

        public CachingClientStore(IdentityServerOptions options, T inner, ICache<Client> cache)
        {
            _options = options;
            _inner = inner;
            _cache = cache;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = await _cache.GetAsync(clientId);

            if (client == null)
            {
                client = await _inner.FindClientByIdAsync(clientId);
                if (client != null)
                {
                    await _cache.SetAsync(clientId, client, _options.CachingOptions.ClientStoreExpiration);
                }
            }

            return client;
        }
    }
}