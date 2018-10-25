// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace IdentityServer4
{
    internal class MessageCookie<TModel>
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IHttpContextAccessor _context;
        private readonly IDataProtector _protector;

        public MessageCookie(
            ILogger<MessageCookie<TModel>> logger, 
            IdentityServerOptions options,
            IHttpContextAccessor context, 
            IDataProtectionProvider provider)
        {
            _logger = logger;
            _options = options;
            _context = context;
            _protector = provider.CreateProtector(MessageType);
        }

        private string MessageType => typeof(TModel).Name;

        private string Protect(Message<TModel> message)
        {
            var json = ObjectSerializer.ToString(message);
            _logger.LogTrace("Protecting message: {0}", json);

            return _protector.Protect(json);
        }

        private Message<TModel> Unprotect(string data)
        {
            var json = _protector.Unprotect(data);
            var message = ObjectSerializer.FromString<Message<TModel>>(json);
            return message;
        }

        private string CookiePrefix => MessageType + ".";

        private string GetCookieFullName(string id)
        {
            return CookiePrefix + id;
        }

        private string CookiePath => _context.HttpContext.GetIdentityServerBasePath().CleanUrlPath();

        private IEnumerable<string> GetCookieNames()
        {
            var key = CookiePrefix;
            foreach (var cookie in _context.HttpContext.Request.Cookies)
            {
                if (cookie.Key.StartsWith(key))
                {
                    yield return cookie.Key;
                }
            }
        }

        private bool Secure => _context.HttpContext.Request.IsHttps;

        public void Write(string id, Message<TModel> message)
        {
            ClearOverflow();

            if (message == null) throw new ArgumentNullException(nameof(message));

            var name = GetCookieFullName(id);
            var data = Protect(message);

            _context.HttpContext.Response.Cookies.Append(
                name,
                data,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath,
                    IsEssential = true,
                    // don't need to set same-site since cookie is expected to be sent
                    // to only another page in this host. 
                });
        }

        public Message<TModel> Read(string id)
        {
            if (id.IsMissing()) return null;

            var name = GetCookieFullName(id);
            return ReadByCookieName(name);
        }

        private Message<TModel> ReadByCookieName(string name)
        {
            var data = _context.HttpContext.Request.Cookies[name];
            if (data.IsPresent())
            {
                try
                {
                    return Unprotect(data);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error unprotecting message cookie");
                    ClearByCookieName(name);
                }
            }
            return null;
        }

        internal protected void Clear(string id)
        {
            var name = GetCookieFullName(id);
            ClearByCookieName(name);
        }

        private void ClearByCookieName(string name)
        {
            _context.HttpContext.Response.Cookies.Append(
                name,
                ".",
                new CookieOptions
                {
                    Expires = new DateTime(2000, 1, 1),
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath,
                    IsEssential = true,
                });
        }

        private long GetCookieRank(string name)
        {   
            // empty and invalid cookies are considered to be the oldest:
            var rank = DateTime.MinValue.Ticks;

            try
            {
                var message = ReadByCookieName(name);
                if (message != null)
                {
                    // valid cookies are ranked based on their creation time:
                    rank = message.Created;
                }
            }
            catch (CryptographicException e)
            {   
                // cookie was protected with a different key/algorithm
                _logger.LogDebug(e, "Unable to unprotect cookie {0}", name);
            }
            
            return rank;
        }

        private void ClearOverflow()
        {
            var names = GetCookieNames();
            var toKeep = _options.UserInteraction.CookieMessageThreshold;

            if (names.Count() >= toKeep)
            {
                var rankedCookieNames =
                    from name in names
                    let rank = GetCookieRank(name)
                    orderby rank descending
                    select name;

                var purge = rankedCookieNames.Skip(Math.Max(0, toKeep - 1));
                foreach (var name in purge)
                {
                    _logger.LogTrace("Purging stale cookie: {cookieName}", name);
                    ClearByCookieName(name);
                }
            }
        }
    }
}