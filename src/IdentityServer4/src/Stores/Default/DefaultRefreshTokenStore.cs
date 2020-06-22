// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using System.Threading;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Default refresh token store.
    /// </summary>
    public class DefaultRefreshTokenStore : DefaultGrantStore<RefreshToken>, IRefreshTokenStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRefreshTokenStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultRefreshTokenStore(
            IPersistedGrantStore store, 
            IPersistentGrantSerializer serializer, 
            IHandleGenerationService handleGenerationService,
            ILogger<DefaultRefreshTokenStore> logger) 
            : base(IdentityServerConstants.PersistedGrantTypes.RefreshToken, store, serializer, handleGenerationService, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            return await CreateItemAsync(refreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.SessionId, refreshToken.Description, refreshToken.CreationTime, refreshToken.Lifetime, cancellationToken);
        }

        /// <inheritdoc/>
        public Task UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            return StoreItemAsync(handle, refreshToken, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.SessionId, refreshToken.Description, refreshToken.CreationTime, refreshToken.CreationTime.AddSeconds(refreshToken.Lifetime), refreshToken.ConsumedTime, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle, CancellationToken cancellationToken = default)
        {
            return GetItemAsync(refreshTokenHandle, cancellationToken);
        }

        /// <inheritdoc/>
        public Task RemoveRefreshTokenAsync(string refreshTokenHandle, CancellationToken cancellationToken = default)
        {
            return RemoveItemAsync(refreshTokenHandle, cancellationToken);
        }

        /// <inheritdoc/>
        public Task RemoveRefreshTokensAsync(string subjectId, string clientId, CancellationToken cancellationToken = default)
        {
            return RemoveAllAsync(subjectId, clientId, cancellationToken);
        }
    }
}