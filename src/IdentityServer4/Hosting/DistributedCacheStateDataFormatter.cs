// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    // todo: review all of this mess and maybe move to infrastructure?
    public static class DistributedCacheStateDataFormatterExtensions
    {
        public static ISecureDataFormat<AuthenticationProperties> CreateDistributedCacheStateDataFormatter<TOptions>(this IServiceCollection services, string name)
            where TOptions : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            services.AddSingleton<IPostConfigureOptions<TOptions>, DistributedCacheStateDataFormatterInitializer<TOptions>>();
            return new DistributedCacheStateDataFormatterMarker(name);
        }

        public static void AddDistributedCacheStateDataFormatterForOidc(this IServiceCollection services)
        {
            var typeName = "Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions, Microsoft.AspNetCore.Authentication.OpenIdConnect";
            var optionsType = Type.GetType(typeName);
            var serviceType = typeof(IPostConfigureOptions<>).MakeGenericType(optionsType);
            var implType = typeof(DistributedCacheStateDataFormatterInitializerForAll<>).MakeGenericType(optionsType);
            services.AddSingleton(serviceType, implType);
        }
    }
}

namespace IdentityServer4.Hosting
{
    class DistributedCacheStateDataFormatterInitializerForAll<TOptions> : IPostConfigureOptions<TOptions>
         where TOptions : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DistributedCacheStateDataFormatterInitializerForAll(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void PostConfigure(string name, TOptions options)
        {
            dynamic opts = options;
            opts.StateDataFormat = new DistributedCacheStateDataFormatter(_httpContextAccessor, name);
        }
    }

    class DistributedCacheStateDataFormatterInitializer<TOptions> : IPostConfigureOptions<TOptions>
        where TOptions : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DistributedCacheStateDataFormatterInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void PostConfigure(string name, TOptions options)
        {
            dynamic opts = options;
            if (opts.StateDataFormat is DistributedCacheStateDataFormatterMarker marker)
            {
                if (marker.Name == name)
                {
                    opts.StateDataFormat = new DistributedCacheStateDataFormatter(_httpContextAccessor, marker.Name);
                }
            }
        }
    }

    class DistributedCacheStateDataFormatterMarker : ISecureDataFormat<AuthenticationProperties>
    {
        public string Name { get; set; }

        public DistributedCacheStateDataFormatterMarker(string name)
        {
            Name = name;
        }

        public string Protect(AuthenticationProperties data)
        {
            throw new NotImplementedException();
        }

        public string Protect(AuthenticationProperties data, string purpose)
        {
            throw new NotImplementedException();
        }

        public AuthenticationProperties Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }

        public AuthenticationProperties Unprotect(string protectedText, string purpose)
        {
            throw new NotImplementedException();
        }
    }

    public class DistributedCacheStateDataFormatter : ISecureDataFormat<AuthenticationProperties>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _name;

        public DistributedCacheStateDataFormatter(IHttpContextAccessor httpContext, string name)
        {
            _httpContext = httpContext;
            _name = name;
        }

        string CacheKeyPrefix => "DistributedCacheStateDataFormatter";

        IDistributedCache Cache => _httpContext.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        IDataProtector Protector => _httpContext.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector(CacheKeyPrefix, _name);

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
