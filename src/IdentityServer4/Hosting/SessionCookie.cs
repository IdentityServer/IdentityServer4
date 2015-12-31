// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Http;
using System;
using System.Linq;

namespace IdentityServer4.Core.Hosting
{
    class SessionCookie
    {
        private readonly IdentityServerContext _context;

        public SessionCookie(IdentityServerContext context)
        {
            _context = context;
        }

        public virtual string IssueSessionId(bool? persistent = null, DateTimeOffset? expires = null)
        {
            var id = CryptoRandom.CreateUniqueId();

            _context.HttpContext.Response.Cookies.Append(
                GetCookieName(),
                id, 
                CreateCookieOptions(persistent, expires));

            return id;
        }

        private CookieOptions CreateCookieOptions(bool? persistent, DateTimeOffset? expires = null)
        {
            var secure = _context.HttpContext.Request.IsHttps;
            var path = _context.GetBasePath().CleanUrlPath();

            var options = new CookieOptions
            {
                HttpOnly = false,
                Secure = secure,
                Path = path
            };

            // todo: load authN cookie and copy its values for persistent/expiration
            //if (persistent != false)
            //{
            //    if (persistent == true || _context.Options.AuthenticationOptions.CookieAuthenticationOptions.IsPersistent)
            //    {
            //        if (persistent == true)
            //        {
            //            expires = expires ?? DateTimeHelper.UtcNow.Add(this.identityServerOptions.AuthenticationOptions.CookieOptions.RememberMeDuration);
            //        }
            //        else
            //        {
            //            expires = expires ?? DateTimeHelper.UtcNow.Add(this.identityServerOptions.AuthenticationOptions.CookieOptions.ExpireTimeSpan);
            //        }
            //        options.Expires = expires.Value.UtcDateTime;
            //    }
            //}

            return options;
        }

        private string GetCookieName()
        {
            // todo
            return "idsvr.session";
            //return identityServerOptions.AuthenticationOptions.CookieOptions.GetSessionCookieName();
        }

        public virtual string GetSessionId()
        {
            // todo: don't re-use session id when authN changes
            // this might be hard to detect since we're relying upon
            // external authN pages

            if (_context.HttpContext.Request.Cookies.ContainsKey(GetCookieName()))
            {
                return _context.HttpContext.Request.Cookies[GetCookieName()].FirstOrDefault().ToString();
            }

            return IssueSessionId();
        }

        public virtual void ClearSessionId()
        {
            var name = GetCookieName();
            if (_context.HttpContext.Request.Cookies.ContainsKey(name))
            {
                // only remove it if we have it in the request
                var options = CreateCookieOptions(false);
                options.Expires = DateTimeHelper.UtcNow.AddYears(-1);

                _context.HttpContext.Response.Cookies.Append(name, ".", options);
            }
        }
    }
}
