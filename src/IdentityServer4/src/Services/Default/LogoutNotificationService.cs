﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default implementation of logout notification service.
    /// </summary>
    public class LogoutNotificationService : ILogoutNotificationService
    {
        private readonly IClientStore _clientStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LogoutNotificationService> _logger;


        /// <summary>
        /// Ctor.
        /// </summary>
        public LogoutNotificationService(
            IClientStore clientStore,
            IHttpContextAccessor httpContextAccessor, 
            ILogger<LogoutNotificationService> logger)
        {
            _clientStore = clientStore;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetFrontChannelLogoutNotificationsUrlsAsync(LogoutNotificationContext context)
        {
            var frontChannelUrls = new List<string>();
            foreach (var clientId in context.ClientIds)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(clientId);
                if (client != null)
                {
                    if (client.FrontChannelLogoutUri.IsPresent())
                    {
                        var url = client.FrontChannelLogoutUri;

                        // add session id if required
                        if (client.ProtocolType == IdentityServerConstants.ProtocolTypes.OpenIdConnect)
                        {
                            if (client.FrontChannelLogoutSessionRequired)
                            {
                                url = url.AddQueryString(OidcConstants.EndSessionRequest.Sid, context.SessionId);
                                url = url.AddQueryString(OidcConstants.EndSessionRequest.Issuer, _httpContextAccessor.HttpContext.GetIdentityServerIssuerUri());
                            }
                        }
                        else if (client.ProtocolType == IdentityServerConstants.ProtocolTypes.WsFederation)
                        {
                            url = url.AddQueryString(Constants.WsFedSignOut.LogoutUriParameterName, Constants.WsFedSignOut.LogoutUriParameterValue);
                        }

                        frontChannelUrls.Add(url);
                    }
                }
            }

            if (frontChannelUrls.Any())
            {
                var msg = frontChannelUrls.Aggregate((x, y) => x + ", " + y);
                _logger.LogDebug("Client front-channel logout URLs: {0}", msg);
            }
            else
            {
                _logger.LogDebug("No client front-channel logout URLs");
            }

            return frontChannelUrls;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BackChannelLogoutRequest>> GetBackChannelLogoutNotificationsAsync(LogoutNotificationContext context)
        {
            var backChannelLogouts = new List<BackChannelLogoutRequest>();
            foreach (var clientId in context.ClientIds)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(clientId);
                if (client != null)
                {
                    if (client.BackChannelLogoutUri.IsPresent())
                    {
                        var back = new BackChannelLogoutRequest
                        {
                            ClientId = clientId,
                            LogoutUri = client.BackChannelLogoutUri,
                            SubjectId = context.SubjectId,
                            SessionId = context.SessionId,
                            SessionIdRequired = client.BackChannelLogoutSessionRequired
                        };

                        backChannelLogouts.Add(back);
                    }
                }
            }

            if (backChannelLogouts.Any())
            {
                var msg = backChannelLogouts.Select(x => x.LogoutUri).Aggregate((x, y) => x + ", " + y);
                _logger.LogDebug("Client back-channel logout URLs: {0}", msg);
            }
            else
            {
                _logger.LogDebug("No client back-channel logout URLs");
            }

            return backChannelLogouts;
        }
    }
}
