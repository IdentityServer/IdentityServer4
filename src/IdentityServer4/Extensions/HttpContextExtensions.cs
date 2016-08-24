﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Hosting
{
    public static class HttpContextExtensions
    {
        public static void SetHost(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.EnvironmentKeys.IdentityServerHost] = value;
        }

        public static void SetBasePath(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] = value;
        }

        public static string GetHost(this HttpContext context)
        {
            return context.Items[Constants.EnvironmentKeys.IdentityServerHost] as string;
        }

        /// <summary>
        /// Gets the base path of IdentityServer. Can be used inside of Katana <c>Map</c>ped middleware.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetBasePath(this HttpContext context)
        {
            return context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] as string;
        }

        /// <summary>
        /// Gets the public base URL for IdentityServer.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerBaseUrl(this HttpContext context)
        {
            return context.GetHost() + context.GetBasePath();
        }

        public static string GetIssuerUri(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var uri = options.IssuerUri;
            if (uri.IsMissing())
            {
                uri = context.GetIdentityServerBaseUrl();
                if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
                uri = uri.ToLowerInvariant();
            }

            return uri;
        }

        internal static async Task<ClaimsPrincipal> GetIdentityServerUserAsync(this HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            return await context.Authentication.AuthenticateAsync(options.AuthenticationOptions.EffectiveAuthenticationScheme);
        }

        internal static string GetIdentityServerSignoutFrameCallbackUrl(this HttpContext context)
        {
            var sessionCookie = context.RequestServices.GetRequiredService<SessionCookie>();
            var sid = sessionCookie.GetSessionId();
            if (sid != null)
            {
                var signoutIframeUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.EndSessionCallback;
                //TODO: update sid to OidcConstants when idmodel released
                signoutIframeUrl = signoutIframeUrl.AddQueryString("sid" + "=" + sid);
                return signoutIframeUrl;
            }
            return null;
        }
    }
}