// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Configuration
{
    internal class ConfigureInternalCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IdentityServerOptions _idsrv;

        public ConfigureInternalCookieOptions(IdentityServerOptions idsrv)
        {
            _idsrv = idsrv;
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name == IdentityServerConstants.DefaultCookieAuthenticationScheme)
            {
                options.SlidingExpiration = _idsrv.Authentication.CookieSlidingExpiration;
                options.ExpireTimeSpan = _idsrv.Authentication.CookieLifetime;
                options.Cookie.Name = IdentityServerConstants.DefaultCookieAuthenticationScheme;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;

                options.LoginPath = ExtractLocalUrl(_idsrv.UserInteraction.LoginUrl);
                options.LogoutPath = ExtractLocalUrl(_idsrv.UserInteraction.LogoutUrl);
                if (_idsrv.UserInteraction.LoginReturnUrlParameter != null)
                {
                    options.ReturnUrlParameter = _idsrv.UserInteraction.LoginReturnUrlParameter;
                }
            }

            if (name == IdentityServerConstants.ExternalCookieAuthenticationScheme)
            {
                options.Cookie.Name = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.Cookie.IsEssential = true;
                // https://github.com/IdentityServer/IdentityServer4/issues/2595
                // need to set None because iOS 12 safari considers the POST back to the client from the 
                // IdP as not safe, so cookies issued from response (with lax) then should not be honored.
                // so we need to make those cookies issued without same-site, thus the browser will
                // hold onto them and send on the next redirect to the callback page.
                options.Cookie.SameSite = SameSiteMode.None;
            }
        }

        private static string ExtractLocalUrl(string url)
        {
            if (url.IsLocalUrl())
            {
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(1);
                }

                return url;
            }

            return null;
        }
    }

    internal class PostConfigureInternalCookieOptions : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IdentityServerOptions _idsrv;
        private readonly IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> _authOptions;
        private readonly ILogger _logger;

        public PostConfigureInternalCookieOptions(
            IdentityServerOptions idsrv,
            IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions,
            ILoggerFactory loggerFactory)
        {
            _idsrv = idsrv;
            _authOptions = authOptions;
            _logger = loggerFactory.CreateLogger("IdentityServer4.Startup");
        }

        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            var scheme = _idsrv.Authentication.CookieAuthenticationScheme ??
                _authOptions.Value.DefaultAuthenticateScheme ??
                _authOptions.Value.DefaultScheme;

            if (name == scheme)
            {
                _idsrv.UserInteraction.LoginUrl = _idsrv.UserInteraction.LoginUrl ?? options.LoginPath;
                _idsrv.UserInteraction.LoginReturnUrlParameter = _idsrv.UserInteraction.LoginReturnUrlParameter ?? options.ReturnUrlParameter;
                _idsrv.UserInteraction.LogoutUrl = _idsrv.UserInteraction.LogoutUrl ?? options.LogoutPath;

                _logger.LogDebug("Login Url: {url}", _idsrv.UserInteraction.LoginUrl);
                _logger.LogDebug("Login Return Url Parameter: {param}", _idsrv.UserInteraction.LoginReturnUrlParameter);
                _logger.LogDebug("Logout Url: {url}", _idsrv.UserInteraction.LogoutUrl);

                _logger.LogDebug("ConsentUrl Url: {url}", _idsrv.UserInteraction.ConsentUrl);
                _logger.LogDebug("Consent Return Url Parameter: {param}", _idsrv.UserInteraction.ConsentReturnUrlParameter);

                _logger.LogDebug("Error Url: {url}", _idsrv.UserInteraction.ErrorUrl);
                _logger.LogDebug("Error Id Parameter: {param}", _idsrv.UserInteraction.ErrorIdParameter);
            }
        }
    }
}