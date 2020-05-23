// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default back-channel logout notification implementation.
    /// </summary>
    public class DefaultBackChannelLogoutService : IBackChannelLogoutService
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
        /// The ILogoutNotificationService to build the back channel logout requests.
        /// </summary>
        public ILogoutNotificationService LogoutNotificationService { get; }

        /// <summary>
        /// HttpClient to make the outbound HTTP calls.
        /// </summary>
        protected IBackChannelLogoutHttpClient HttpClient { get; }

        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger<IBackChannelLogoutService> Logger { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="tools"></param>
        /// <param name="logoutNotificationService"></param>
        /// <param name="backChannelLogoutHttpClient"></param>
        /// <param name="logger"></param>
        public DefaultBackChannelLogoutService(
            ISystemClock clock,
            IdentityServerTools tools,
            ILogoutNotificationService logoutNotificationService,
            IBackChannelLogoutHttpClient backChannelLogoutHttpClient,
            ILogger<IBackChannelLogoutService> logger)
        {
            Clock = clock;
            Tools = tools;
            LogoutNotificationService = logoutNotificationService;
            HttpClient = backChannelLogoutHttpClient;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task SendLogoutNotificationsAsync(LogoutNotificationContext context)
        {
            var backChannelRequests = await LogoutNotificationService.GetBackChannelLogoutNotificationsAsync(context);
            if (backChannelRequests.Any())
            {
                await SendLogoutNotificationsAsync(backChannelRequests);
            }
        }

        /// <summary>
        /// Sends the logout notifications for the collection of clients.
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        protected virtual Task SendLogoutNotificationsAsync(IEnumerable<BackChannelLogoutRequest> requests)
        {
            requests = requests ?? Enumerable.Empty<BackChannelLogoutRequest>();
            var tasks = requests.Select(SendLogoutNotificationAsync).ToArray();
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Performs the back-channel logout for a single client.
        /// </summary>
        /// <param name="request"></param>
        protected virtual async Task SendLogoutNotificationAsync(BackChannelLogoutRequest request)
        {
            var data = await CreateFormPostPayloadAsync(request);
            await PostLogoutJwt(request, data);
        }

        /// <summary>
        /// Performs the HTTP POST of the logout payload to the client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Task PostLogoutJwt(BackChannelLogoutRequest client, Dictionary<string, string> data)
        {
            return HttpClient.PostAsync(client.LogoutUri, data);
        }

        /// <summary>
        /// Creates the form-url-encoded payload (as a dictionary) to send to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async Task<Dictionary<string, string>> CreateFormPostPayloadAsync(BackChannelLogoutRequest request)
        {
            var token = await CreateTokenAsync(request);

            var data = new Dictionary<string, string>
            {
                { OidcConstants.BackChannelLogoutRequest.LogoutToken, token }
            };
            return data;
        }

        /// <summary>
        /// Creates the JWT used for the back-channel logout notification.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The token.</returns>
        protected virtual async Task<string> CreateTokenAsync(BackChannelLogoutRequest request)
        {
            var claims = await CreateClaimsForTokenAsync(request);
            if (claims.Any(x => x.Type == JwtClaimTypes.Nonce))
            {
                throw new InvalidOperationException("nonce claim is not allowed in the back-channel signout token.");
            }

            return await Tools.IssueJwtAsync(DefaultLogoutTokenLifetime, claims);
        }

        /// <summary>
        /// Create the claims to be used in the back-channel logout token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The claims to include in the token.</returns>
        protected Task<IEnumerable<Claim>> CreateClaimsForTokenAsync(BackChannelLogoutRequest request)
        {
            if (request.SessionIdRequired && request.SessionId == null)
            {
                throw new ArgumentException("Client requires SessionId", nameof(request.SessionId));
            }

            var json = "{\"" + OidcConstants.Events.BackChannelLogout + "\":{} }";

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, request.SubjectId),
                new Claim(JwtClaimTypes.Audience, request.ClientId),
                new Claim(JwtClaimTypes.IssuedAt, Clock.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)),
                new Claim(JwtClaimTypes.Events, json, IdentityServerConstants.ClaimValueTypes.Json)
            };

            if (request.SessionId != null)
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, request.SessionId));
            }

            return Task.FromResult(claims.AsEnumerable());
        }
    }
}
