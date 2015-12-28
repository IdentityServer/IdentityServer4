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

using IdentityServer4.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Core.Hosting
{
    internal class ClientListCookie
    {
        const string ClientListCookieName = "idsvr.clients";

        static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        private readonly IdentityServerContext _context;

        public ClientListCookie(IdentityServerContext context)
        {
            _context = context;
        }

        public void Clear()
        {
            SetClients(null);
        }

        public void AddClient(string clientId)
        {
            if (_context.Options.Endpoints.EnableEndSessionEndpoint)
            {
                var clients = GetClients();
                if (!clients.Contains(clientId))
                {
                    var update = clients.ToList();
                    update.Add(clientId);
                    SetClients(update);
                }
            }
        }

        public IEnumerable<string> GetClients()
        {
            var value = GetCookie();
            if (String.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }

            return JsonConvert.DeserializeObject<string[]>(value, settings);
        }

        void SetClients(IEnumerable<string> clients)
        {
            string value = null;
            if (clients != null && clients.Any())
            {
                value = JsonConvert.SerializeObject(clients);
            }

            SetCookie(value);
        }

        string CookieName
        {
            get
            {
                // TODO: prefix
                return ClientListCookieName;
            }
        }

        string CookiePath
        {
            get
            {
                return _context.GetBasePath().CleanUrlPath();
            }
        }

        private bool Secure
        {
            get
            {
                return "https".Equals(_context.HttpContext.Request.Scheme, StringComparison.OrdinalIgnoreCase);
            }
        }

        void SetCookie(string value)
        {
            DateTime? expires = null;
            if (String.IsNullOrWhiteSpace(value))
            {
                var existingValue = GetCookie();
                if (existingValue == null)
                {
                    // no need to write cookie to clear if we don't already have one
                    return;
                }

                value = ".";
                expires = DateTime.Now.AddYears(-1);
            }

            var opts = new Microsoft.AspNet.Http.CookieOptions
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
