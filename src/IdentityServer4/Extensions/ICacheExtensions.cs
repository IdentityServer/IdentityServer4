// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Services;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Extensions
{
    public static class ICacheExtensions
    {
        /// <summary>
        /// Attempts to get an item from the cache. If the item is not found, the <c>get</c> function is used to
        /// obtain the item and populate the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="get">The get function.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// cache
        /// or
        /// get
        /// </exception>
        /// <exception cref="System.ArgumentNullException">cache
        /// or
        /// get</exception>
        public static async Task<T> GetAsync<T>(this ICache<T> cache, string key, TimeSpan duration, Func<Task<T>> get)
            where T : class
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (get == null) throw new ArgumentNullException("get");
            if (key == null) return null;

            T item = await cache.GetAsync(key);

            if (item == null)
            {
                item = await get();

                if (item != null)
                {
                    await cache.SetAsync(key, item, duration);
                }
            }

            return item;
        }
    }
}
