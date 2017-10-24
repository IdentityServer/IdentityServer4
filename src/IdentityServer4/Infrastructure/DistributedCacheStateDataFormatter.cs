// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Infrastructure
{
    internal class DistributedCacheStateDataFormatter : ISecureDataFormat<AuthenticationProperties>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _name;

        public DistributedCacheStateDataFormatter(IHttpContextAccessor httpContext, string name)
        {
            _httpContext = httpContext;
            _name = name;
        }

        private string CacheKeyPrefix => "DistributedCacheStateDataFormatter";

        private IDistributedCache Cache => _httpContext.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        private IDataProtector Protector => _httpContext.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector(CacheKeyPrefix, _name);

        public string Protect(AuthenticationProperties data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationProperties data, string purpose)
        {
            var key = Guid.NewGuid().ToString();
            var cacheKey = $"{CacheKeyPrefix}-{purpose}-{key}";
            var json = ObjectSerializer.ToString(data);

            // Rather than encrypt the full AuthenticationProperties
            // cache the data and encrypt the key that points to the data
            Cache.SetString(cacheKey, json);

            return Protector.Protect(key);
        }

        public AuthenticationProperties Unprotect(string protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationProperties Unprotect(string protectedText, string purpose)
        {
            // Decrypt the key and retrieve the data from the cache.
            var key = Protector.Unprotect(protectedText);
            var cacheKey = $"{CacheKeyPrefix}-{purpose}-{key}";
            var json = Cache.GetString(cacheKey);

            return ObjectSerializer.FromString<AuthenticationProperties>(json);
        }
    }
}
