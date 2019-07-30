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
    /// <summary>
    /// State formatter using IDistributedCache
    /// </summary>
    public class DistributedCacheStateDataFormatter : ISecureDataFormat<AuthenticationProperties>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheStateDataFormatter"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context accessor.</param>
        /// <param name="name">The scheme name.</param>
        public DistributedCacheStateDataFormatter(IHttpContextAccessor httpContext, string name)
        {
            _httpContext = httpContext;
            _name = name;
        }

        private string CacheKeyPrefix => "DistributedCacheStateDataFormatter";

        private IDistributedCache Cache => _httpContext.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        private IDataProtector Protector => _httpContext.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector(CacheKeyPrefix, _name);

        /// <summary>
        /// Protects the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public string Protect(AuthenticationProperties data)
        {
            return Protect(data, null);
        }

        /// <summary>
        /// Protects the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="purpose">The purpose.</param>
        /// <returns></returns>
        public string Protect(AuthenticationProperties data, string purpose)
        {
            var key = Guid.NewGuid().ToString();
            var cacheKey = $"{CacheKeyPrefix}-{purpose}-{key}";
            var json = ObjectSerializer.ToString(data);

            var options = new DistributedCacheEntryOptions();
            if (data.ExpiresUtc.HasValue)
            {
                options.SetAbsoluteExpiration(data.ExpiresUtc.Value);
            }
            else
            {
                options.SetSlidingExpiration(Constants.DefaultCacheDuration);
            }
            
            // Rather than encrypt the full AuthenticationProperties
            // cache the data and encrypt the key that points to the data
            Cache.SetString(cacheKey, json, options);

            return Protector.Protect(key);
        }

        /// <summary>
        /// Unprotects the specified protected text.
        /// </summary>
        /// <param name="protectedText">The protected text.</param>
        /// <returns></returns>
        public AuthenticationProperties Unprotect(string protectedText)
        {
            return Unprotect(protectedText, null);
        }

        /// <summary>
        /// Unprotects the specified protected text.
        /// </summary>
        /// <param name="protectedText">The protected text.</param>
        /// <param name="purpose">The purpose.</param>
        /// <returns></returns>
        public AuthenticationProperties Unprotect(string protectedText, string purpose)
        {
            if (String.IsNullOrWhiteSpace(protectedText))
            {
                return null;
            }

            // Decrypt the key and retrieve the data from the cache.
            var key = Protector.Unprotect(protectedText);
            var cacheKey = $"{CacheKeyPrefix}-{purpose}-{key}";
            var json = Cache.GetString(cacheKey);

            if (json == null)
            {
                return null;
            }

            return ObjectSerializer.FromString<AuthenticationProperties>(json);
        }
    }
}
