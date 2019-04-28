// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Infrastructure;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default back-channel signout notification implementation.
    /// </summary>
    public class DefaultBackChannelSignoutService : IBackChannelSignoutService
    {
        /// <summary>
        /// Default value for the back-channel JWT lifetime.
        /// </summary>
        protected const int DefaultLogoutTokenLifetime = 5 * 60;

        /// <summary>
        /// The system clock;
        /// </summary>
        protected ISystemClock Clock { get; }
        /// <summary>
        /// The IdentityServerTools used to create and the JWT.
        /// </summary>
        protected IdentityServerTools Tools { get; }
        /// <summary>
        /// The HttpClient for making the outbound HTTP calls.
        /// </summary>
        protected BackChannelHttpClient BackChannelClient { get; }
        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger<IBackChannelSignoutService> Logger { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="tools"></param>
        /// <param name="backChannelClient"></param>
        /// <param name="logger"></param>
        public DefaultBackChannelSignoutService(
            ISystemClock clock,
            IdentityServerTools tools,
            BackChannelHttpClient backChannelClient,
            ILogger<IBackChannelSignoutService> logger)
        {
            //_httpContext = httpContext;
            Clock = clock;
            Tools = tools;
            BackChannelClient = backChannelClient;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual Task SendSignoutNotificationsAsync(IEnumerable<BackChannelLogoutModel> clients)
        {
            clients = clients ?? Enumerable.Empty<BackChannelLogoutModel>();
            var tasks = clients.Select(x => SendSignoutNotificationAsync(x)).ToArray();
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Performs the back-channel signout for a single client.
        /// </summary>
        /// <param name="client"></param>
        protected virtual async Task SendSignoutNotificationAsync(BackChannelLogoutModel client)
        {
            var token = await CreateTokenAsync(client);

            var data = new Dictionary<string, string>
            {
                { OidcConstants.BackChannelLogoutRequest.LogoutToken, token }
            };

            try
            {
                var response = await BackChannelClient.PostAsync(client.LogoutUri, new FormUrlEncodedContent(data));

                Logger.LogDebug("Back channel logout for client id: {0} to URI: {1}, result: {2}",
                    client.ClientId, client.LogoutUri, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception invoking back channel logout for client id: {0} to URI: {1}", client.ClientId, client.LogoutUri);
            }
        }

        /// <summary>
        /// Creates the JWT used for the back-channel signout notification.
        /// </summary>
        /// <param name="client"></param>
        /// <returns>The token.</returns>
        protected virtual async Task<string> CreateTokenAsync(BackChannelLogoutModel client)
        {
            var claims = await CreateClaimsForTokenAsync(client);
            if (claims.Any(x => x.Type == JwtClaimTypes.Nonce))
            {
                throw new InvalidOperationException("nonce claim is not allowed in the back-channel signout token.");
            }

            return await Tools.IssueJwtAsync(DefaultLogoutTokenLifetime, claims);
        }

        /// <summary>
        /// Create the claims to be used in the back-channel signout token.
        /// </summary>
        /// <param name="client"></param>
        /// <returns>The claims to include in the token.</returns>
        protected Task<IEnumerable<Claim>> CreateClaimsForTokenAsync(BackChannelLogoutModel client)
        {
            var json = "{\"" + OidcConstants.Events.BackChannelLogout + "\":{} }";

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, client.SubjectId),
                new Claim(JwtClaimTypes.Audience, client.ClientId),
                new Claim(JwtClaimTypes.IssuedAt, Clock.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer),
                new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16)),
                new Claim(JwtClaimTypes.Events, json, IdentityServerConstants.ClaimValueTypes.Json)
            };

            if (client.SessionIdRequired)
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, client.SessionId));
            }

            return Task.FromResult(claims.AsEnumerable());
        }
    }
}
