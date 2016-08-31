// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;
using IdentityServer4.Extensions;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services.Default
{
    public class DefaultClientSessionService : IClientSessionService
    {
        const string ClientListKey = "ClientList:";

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        private readonly HttpContext _context;
        private readonly ISessionIdService _sessionId;
        private readonly ILogger<DefaultClientSessionService> _logger;

        public DefaultClientSessionService(IHttpContextAccessor context, ISessionIdService sessionId, ILogger<DefaultClientSessionService> logger)
        {
            _context = context.HttpContext;
            _sessionId = sessionId;
            _logger = logger;
        }

        async Task<string> GetKeyAsync()
        {
            var sid = await _sessionId.GetCurrentSessionIdAsync();
            if (sid == null)
            {
                _logger.LogError("No current session id");
                throw new InvalidOperationException("No current session id");
            }

            return ClientListKey + sid;
        }

        public async Task AddClientIdAsync(string clientId)
        {
            var clients = await GetClientsAsync();
            if (!clients.Contains(clientId))
            {
                var update = clients.ToList();
                update.Add(clientId);

                await SetClientsAsync(update);
            }
        }

        public async Task<IEnumerable<string>> GetClientsAsync()
        {
            var info = await _context.GetIdentityServerUserInfoAsync();
            if (info == null)
            {
                _logger.LogError("No authenticated user");
                throw new InvalidOperationException("No authenticated user");
            }

            var key = await GetKeyAsync();

            if (info.Properties.Items.ContainsKey(key))
            {
                var value = info.Properties.Items[key];
                if (!String.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        var bytes = Base64Url.Decode(value);
                        value = Encoding.UTF8.GetString(bytes);
                        return JsonConvert.DeserializeObject<string[]>(value, settings);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError("Error decoding client list: {0}", ex.Message);
                        // clear so we don't keep failing
                        await SetClientsAsync(null);
                    }
                }
            }

            return Enumerable.Empty<string>();
        }

        async Task SetClientsAsync(IEnumerable<string> clients)
        {
            string value = null;
            if (clients != null && clients.Any())
            {
                value = JsonConvert.SerializeObject(clients);
                var bytes = Encoding.UTF8.GetBytes(value);
                value = Base64Url.Encode(bytes);
            }

            var info = await _context.GetIdentityServerUserInfoAsync();
            if (info == null) throw new InvalidOperationException("No authenticated user");

            var key = await GetKeyAsync();

            if (value == null)
            {
                info.Properties.Items.Remove(key);
            }
            else
            {
                info.Properties.Items[key] = value;
            }

            await _context.ReIssueSignInCookie(info);
        }


        public Task EnsureClientListCookie()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetClientListFromCookie()
        {
            throw new NotImplementedException();
        }

        public Task RemoveCookie()
        {
            throw new NotImplementedException();
        }
    }
}