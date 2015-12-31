/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace IdentityServer4.Core.Hosting
{
    class MessageCookie<TMessage>
        where TMessage : Message
    {
        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        private readonly ILogger<MessageCookie<TMessage>> _logger;
        private readonly IdentityServerContext _context;
        private readonly IDataProtector _protector;

        public MessageCookie(ILogger<MessageCookie<TMessage>> logger, IdentityServerContext context, IDataProtectionProvider provider)
        {
            _logger = logger;
            _context = context;
            _protector = provider.CreateProtector(MessageType);
        }

        string MessageType
        {
            get { return typeof(TMessage).Name; }
        }

        string Protect(TMessage message)
        {
            var json = JsonConvert.SerializeObject(message, settings);
            _logger.LogDebug("Protecting message: {0}", json);

            return _protector.Protect(json);
        }

        TMessage Unprotect(string data)
        {
            var json = _protector.Unprotect(data);
            var message = JsonConvert.DeserializeObject<TMessage>(json);
            return message;
        }

        string GetCookieName(string id = null)
        {
            // TODO: cookie prefix
            //return String.Format("{0}{1}.{2}",
            //    options.AuthenticationOptions.CookieOptions.Prefix,
            //    MessageType,
            //    id);
            return String.Format("idsvr.{0}.{1}", MessageType, id);
        }

        string CookiePath
        {
            get
            {
                return _context.GetIdentityServerBaseUrl().CleanUrlPath();
            }
        }

        private IEnumerable<string> GetCookieNames()
        {
            var key = GetCookieName();
            foreach (var cookie in _context.HttpContext.Request.Cookies)
            {
                if (cookie.Key.StartsWith(key))
                {
                    yield return cookie.Key;
                }
            }
        }

        private bool Secure
        {
            get
            {
                return _context.HttpContext.Request.IsHttps;
            }
        }

        public string Write(TMessage message)
        {
            ClearOverflow();

            if (message == null) throw new ArgumentNullException("message");

            var id = CryptoRandom.CreateUniqueId();
            var name = GetCookieName(id);
            var data = Protect(message);

            _context.HttpContext.Response.Cookies.Append(
                name,
                data,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath
                });
            return id;
        }

        public TMessage Read(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) return null;

            var name = GetCookieName(id);
            return ReadByCookieName(name);
        }

        TMessage ReadByCookieName(string name)
        {
            var data = _context.HttpContext.Request.Cookies[name];
            if (!String.IsNullOrWhiteSpace(data))
            {
                return Unprotect(data);
            }
            return null;
        }

        public void Clear(string id)
        {
            var name = GetCookieName(id);
            ClearByCookieName(name);
        }

        void ClearByCookieName(string name)
        {
            _context.HttpContext.Response.Cookies.Append(
                name,
                ".",
                new CookieOptions
                {
                    Expires = DateTimeHelper.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Secure,
                    Path = CookiePath
                });
        }

        private long GetCookieRank(string name)
        {   
            // empty and invalid cookies are considered to be the oldest:
            var rank = DateTimeOffset.MinValue.Ticks;

            try
            {
                var message = ReadByCookieName(name);
                if (message != null)
                {   // valid cookies are ranked based on their creation time:
                    rank = message.Created;
                }
            }
            catch (CryptographicException e)
            {   
                // cookie was protected with a different key/algorithm
                _logger.LogDebug("Unable to decrypt cookie {0}: {1}", name, e.Message);
            }
            
            return rank;
        }

        private void ClearOverflow()
        {
            var names = GetCookieNames();
            var toKeep = _context.Options.AuthenticationOptions.SignInMessageThreshold;

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
                    ClearByCookieName(name);
                }
            }
        }
    }
}