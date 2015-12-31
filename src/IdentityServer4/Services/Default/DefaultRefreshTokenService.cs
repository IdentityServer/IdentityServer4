// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.Default
{
    /// <summary>
    /// Default refresh token service
    /// </summary>
    public class DefaultRefreshTokenService : IRefreshTokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The refresh token store
        /// </summary>
        protected readonly IRefreshTokenStore _store;

        /// <summary>
        /// The _events
        /// </summary>
        protected readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRefreshTokenService" /> class.
        /// </summary>
        /// <param name="store">The refresh token store.</param>
        /// <param name="events">The events.</param>
        public DefaultRefreshTokenService(IRefreshTokenStore store, IEventService events, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DefaultRefreshTokenService>();
            _store = store;
            _events = events;
        }

        /// <summary>
        /// Creates the refresh token.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        public virtual async Task<string> CreateRefreshTokenAsync(ClaimsPrincipal subject, Token accessToken, Client client)
        {
            _logger.LogVerbose("Creating refresh token");

            int lifetime;
            if (client.RefreshTokenExpiration == TokenExpiration.Absolute)
            {
                _logger.LogVerbose("Setting an absolute lifetime: " + client.AbsoluteRefreshTokenLifetime);
                lifetime = client.AbsoluteRefreshTokenLifetime;
            }
            else
            {
                _logger.LogVerbose("Setting a sliding lifetime: " + client.SlidingRefreshTokenLifetime);
                lifetime = client.SlidingRefreshTokenLifetime;
            }

            var handle = CryptoRandom.CreateUniqueId();
            var refreshToken = new RefreshToken
            {
                CreationTime = DateTimeOffsetHelper.UtcNow,
                LifeTime = lifetime,
                AccessToken = accessToken,
                Subject = subject
            };

            await _store.StoreAsync(handle, refreshToken);

            await RaiseRefreshTokenIssuedEventAsync(handle, refreshToken);
            return handle;
        }

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        public virtual async Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client)
        {
            _logger.LogVerbose("Updating refresh token");

            bool needsUpdate = false;

            if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
            {
                _logger.LogVerbose("Token usage is one-time only. Generating new handle");

                // delete old one
                await _store.RemoveAsync(handle);

                // create new one
                handle = CryptoRandom.CreateUniqueId();
                needsUpdate = true;
            }

            if (client.RefreshTokenExpiration == TokenExpiration.Sliding)
            {
                _logger.LogVerbose("Refresh token expiration is sliding - extending lifetime");

                // make sure we don't exceed absolute exp
                // cap it at absolute exp
                var currentLifetime = refreshToken.CreationTime.GetLifetimeInSeconds();
                _logger.LogVerbose("Current lifetime: " + currentLifetime.ToString());

                var newLifetime = currentLifetime + client.SlidingRefreshTokenLifetime;
                _logger.LogVerbose("New lifetime: " + newLifetime.ToString());

                if (newLifetime > client.AbsoluteRefreshTokenLifetime)
                {
                    newLifetime = client.AbsoluteRefreshTokenLifetime;
                    _logger.LogVerbose("New lifetime exceeds absolute lifetime, capping it to " + newLifetime.ToString());
                }

                refreshToken.LifeTime = newLifetime;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _store.StoreAsync(handle, refreshToken);
                _logger.LogVerbose("Updated refresh token in store");
            }
            else
            {
                _logger.LogVerbose("No updates to refresh token done");
            }

            await RaiseRefreshTokenRefreshedEventAsync(handle, handle, refreshToken);
            _logger.LogVerbose("No updates to refresh token done");

            return handle;
        }

        /// <summary>
        /// Raises the refresh token issued event.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="token">The token.</param>
        protected async Task RaiseRefreshTokenIssuedEventAsync(string handle, RefreshToken token)
        {
            await _events.RaiseRefreshTokenIssuedEventAsync(handle, token);
        }

        /// <summary>
        /// Raises the refresh token refreshed event.
        /// </summary>
        /// <param name="oldHandle">The old handle.</param>
        /// <param name="newHandle">The new handle.</param>
        /// <param name="token">The token.</param>
        protected async Task RaiseRefreshTokenRefreshedEventAsync(string oldHandle, string newHandle, RefreshToken token)
        {
            await _events.RaiseSuccessfulRefreshTokenRefreshEventAsync(oldHandle, newHandle, token);
        }
    }
}