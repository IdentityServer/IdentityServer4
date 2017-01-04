// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http.Features.Authentication;
using IdentityServer4.Extensions;

namespace IdentityServer4.Services
{
    public class DefaultSessionIdService : ISessionIdService
    {
        private readonly IHttpContextAccessor _context;
        private readonly IdentityServerOptions _options;

        public DefaultSessionIdService(IHttpContextAccessor context, IdentityServerOptions options)
        {
            _context = context;
            _options = options;
        }

        public void CreateSessionId(SignInContext context)
        {
            if (!context.Properties.ContainsKey(OidcConstants.EndSessionRequest.Sid))
            {
                context.Properties[OidcConstants.EndSessionRequest.Sid] = CryptoRandom.CreateUniqueId(16);
            }

            IssueSessionIdCookie(context.Properties[OidcConstants.EndSessionRequest.Sid]);
        }

        public async Task<string> GetCurrentSessionIdAsync()
        {
            var info = await _context.HttpContext.GetIdentityServerUserInfoAsync();
            if (info.Properties.Items.ContainsKey(OidcConstants.EndSessionRequest.Sid))
            {
                var sid = info.Properties.Items[OidcConstants.EndSessionRequest.Sid];
                return sid;
            }
            return null;
        }

        public async Task EnsureSessionCookieAsync()
        {
            var sid = await GetCurrentSessionIdAsync();
            if (sid != null)
            {
                IssueSessionIdCookie(sid);
            }
            else
            {
                // we don't want to delete the session id cookie if the user is
                // no longer authenticated since we might be waiting for the 
                // signout iframe to render -- it's a timing issue between the 
                // logout page removing the authentication cookie and the 
                // signout iframe callback from performing SLO
            }
        }

        public string GetCookieName()
        {
            return $"{_options.Authentication.EffectiveAuthenticationScheme}.session";
        }

        public string GetCookieValue()
        {
            return _context.HttpContext.Request.Cookies[GetCookieName()];
        }

        public void RemoveCookie()
        {
            var name = GetCookieName();
            if (_context.HttpContext.Request.Cookies.ContainsKey(name))
            {
                // only remove it if we have it in the request
                var options = CreateCookieOptions();
                options.Expires = IdentityServerDateTime.UtcNow.AddYears(-1);

                _context.HttpContext.Response.Cookies.Append(name, ".", options);
            }
        }

        void IssueSessionIdCookie(string sid)
        {
            if (GetCookieValue() != sid)
            {
                _context.HttpContext.Response.Cookies.Append(
                    GetCookieName(),
                    sid,
                    CreateCookieOptions());
            }
        }

        CookieOptions CreateCookieOptions()
        {
            var secure = _context.HttpContext.Request.IsHttps;
            var path = _context.HttpContext.GetIdentityServerBasePath().CleanUrlPath();

            var options = new CookieOptions
            {
                HttpOnly = false,
                Secure = secure,
                Path = path
            };

            return options;
        }
    }
}