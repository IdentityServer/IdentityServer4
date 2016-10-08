using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation.Caching
{
    public class CachingTokenValidator<T> : ITokenValidator
        where T : ITokenValidator
    {
        private readonly ICache<TokenValidationResult> _cache;
        private readonly T _inner;
        private readonly ILogger<CachingTokenValidator<T>> _logger;
        private readonly IdentityServerOptions _options;

        public CachingTokenValidator(IdentityServerOptions options, T inner, ICache<TokenValidationResult> cache, ILogger<CachingTokenValidator<T>> logger)
        {
            _options = options;
            _inner = inner;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            var key = $"access_token:{token}:{expectedScope}";
            key = key.Sha256();

            var result = await _cache.GetAsync(key,
                _options.CachingOptions.TokenValidationExpiration,
                () => _inner.ValidateAccessTokenAsync(token, expectedScope),
                _logger);

            return result;
        }

        public async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            var key = $"id_token:{token}:{clientId}:{validateLifetime}";
            key = key.Sha256();

            var result = await _cache.GetAsync(key,
                _options.CachingOptions.TokenValidationExpiration,
                () => _inner.ValidateIdentityTokenAsync(token, clientId, validateLifetime),
                _logger);

            return result;
        }
    }
}
