﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Infrastructure
{
    internal class BackChannelLogoutClient
    {
        const int LogoutTokenLifetime = 5 * 60;

        //private readonly IHttpContextAccessor _httpContext;
        private readonly IdentityServerTools _tools;
        private readonly HttpClient _backChannelClient;
        private readonly ILogger<BackChannelLogoutClient> _logger;

        public BackChannelLogoutClient(
            //IHttpContextAccessor httpContext,
            IdentityServerTools tools,
            HttpClient backChannelClient,
            ILogger<BackChannelLogoutClient> logger)
        {
            //_httpContext = httpContext;
            _tools = tools;
            _backChannelClient = backChannelClient;
            _logger = logger;
        }

        public virtual Task SendLogoutsAsync(IEnumerable<BackChannelLogoutModel> clients)
        {
            clients = clients ?? Enumerable.Empty<BackChannelLogoutModel>();
            var tasks = clients.Select(x => SendLogoutAsync(x));
            return Task.WhenAll(tasks);
        }

        async Task SendLogoutAsync(BackChannelLogoutModel client)
        {
            var token = await CreateLogoutTokenAsync(client);

            var data = new Dictionary<string, string>
            {
                { OidcConstants.BackChannelLogoutRequest.LogoutToken, token }
            };

            try
            {
                var response = await _backChannelClient.PostAsync(client.LogoutUri, new FormUrlEncodedContent(data));

                _logger.LogDebug("Back channel logout for client id: {0} to URI: {1}, result: {2}",
                    client.ClientId, client.LogoutUri, (int)response.StatusCode);
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception invoking back channel logout for client id: {0} to URI: {1}, message: {2}", client.ClientId, client.LogoutUri, ex.Message);
            }
        }

        async Task<string> CreateLogoutTokenAsync(BackChannelLogoutModel client)
        {
            var json = "{\"" + OidcConstants.Events.BackChannelLogout + "\":{} }";
            var claims = new List<Claim>()
            {
                //new Claim(JwtClaimTypes.Issuer, _httpContext.HttpContext.GetIdentityServerIssuerUri()),
                new Claim(JwtClaimTypes.Subject, client.SubjectId),
                new Claim(JwtClaimTypes.Audience, client.ClientId),
                new Claim(JwtClaimTypes.IssuedAt, IdentityServerDateTime.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer),
                new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16)),
                new Claim(JwtClaimTypes.Events, json, IdentityServerConstants.ClaimValueTypes.Json),
            };

            if (client.SessionIdRequired)
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, client.SessionId));
            }

            return await _tools.IssueJwtAsync(LogoutTokenLifetime, claims);
        }
    }
}
