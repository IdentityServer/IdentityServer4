// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using IdentityServer4.Extensions;

namespace IdentityServer4.Services.Default
{
    public class DefaultClientSessionService : IClientSessionService
    {
        const string ClientListKey = "ClientSessions";

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        private readonly IHttpContextAccessor _context;
        private readonly ISessionIdService _sessionId;
        private readonly ILogger<DefaultClientSessionService> _logger;

        public DefaultClientSessionService(IHttpContextAccessor context, ISessionIdService sessionId, ILogger<DefaultClientSessionService> logger)
        {
            _context = context;
            _sessionId = sessionId;
            _logger = logger;
        }

        string GetKey()
        {
            return ClientListKey;
        }

        public async Task AddClientIdAsync(string clientId)
        {
            var clients = await GetClientListAsync();
            if (!clients.Contains(clientId))
            {
                var update = clients.ToList();
                update.Add(clientId);

                await SetClientsAsync(update);
            }
        }

        public async Task<IEnumerable<string>> GetClientListAsync()
        {
            var value = await GetPropertyValueAsync();
            try
            {
                return DecodeList(value);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error decoding client list: {0}", ex.Message);
                // clear so we don't keep failing
                await SetClientsAsync(null);
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> DecodeList(string value)
        {
            if (value.IsPresent())
            {
                var bytes = Base64Url.Decode(value);
                value = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<string[]>(value, settings);
            }

            return Enumerable.Empty<string>();
        }

        public string EncodeList(IEnumerable<string> list)
        {
            if (list != null && list.Any())
            {
                var value = JsonConvert.SerializeObject(list);
                var bytes = Encoding.UTF8.GetBytes(value);
                value = Base64Url.Encode(bytes);
                return value;
            }
            return null;
        }

        async Task<string> GetPropertyValueAsync()
        {
            var info = await _context.HttpContext.GetIdentityServerUserInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("No authenticated user");
                return null;
            }

            var key = GetKey();

            if (info.Properties.Items.ContainsKey(key))
            {
                var value = info.Properties.Items[key];
                return value;
            }

            return null;
        }

        async Task SetClientsAsync(IEnumerable<string> clients)
        {
            var value = EncodeList(clients);
            await SetPropertyValueAsync(value);
        }

        async Task SetPropertyValueAsync(string value)
        {
            var info = await _context.HttpContext.GetIdentityServerUserInfoAsync();
            if (info == null)
            {
                _logger.LogError("No authenticated user");
                throw new InvalidOperationException("No authenticated user");
            }

            var key = GetKey();

            if (value == null)
            {
                info.Properties.Items.Remove(key);
            }
            else
            {
                info.Properties.Items[key] = value;
            }

            await _context.HttpContext.ReIssueSignInCookie(info);
        }

        public async Task EnsureClientListCookieAsync()
        {
            var value = await GetPropertyValueAsync();
            SetCookie(value);
        }

        public IEnumerable<string> GetClientListFromCookie()
        {
            var value = GetCookie();
            var list = DecodeList(value);
            return list;
        }

        public void RemoveCookie()
        {
            SetCookie(null);
        }

        string CookieName => ClientListKey + "." + _sessionId.GetCookieValue();

        string CookiePath => _context.HttpContext.GetBasePath().CleanUrlPath();

        private bool Secure => _context.HttpContext.Request.IsHttps;

        void SetCookie(string value)
        {
            DateTime? expires = null;
            if (value.IsMissing())
            {
                var existingValue = GetCookie();
                if (existingValue == null)
                {
                    // no need to write cookie to clear if we don't already have one
                    return;
                }

                value = ".";
                expires = DateTimeHelper.UtcNow.AddYears(-1);
            }

            var opts = new CookieOptions
            {
                HttpOnly = true,
                Secure = Secure,
                Path = CookiePath,
                Expires = expires
            };

            _context.HttpContext.Response.Cookies.Append(CookieName, value, opts);
        }

        string GetCookie()
        {
            return _context.HttpContext.Request.Cookies[CookieName];
        }
    }
}