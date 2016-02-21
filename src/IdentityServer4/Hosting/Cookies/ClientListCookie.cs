// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public virtual void AddClient(string clientId)
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

            var bytes = IdentityModel.Base64Url.Decode(value);
            value = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<string[]>(value, settings);
        }

        void SetClients(IEnumerable<string> clients)
        {
            string value = null;
            if (clients != null && clients.Any())
            {
                value = JsonConvert.SerializeObject(clients);
                var bytes = Encoding.UTF8.GetBytes(value);
                value = IdentityModel.Base64Url.Encode(bytes);
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
                return _context.HttpContext.Request.IsHttps;
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
