// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Linq;
using System.Collections.Generic;

#pragma warning disable 1591

namespace IdentityServer4.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetIdentityServerOrigin(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.EnvironmentKeys.IdentityServerOrigin] = value;
        }

        public static void SetIdentityServerBasePath(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] = value;
        }

        public static string GetIdentityServerOrigin(this HttpContext context)
        {
            return context.Items[Constants.EnvironmentKeys.IdentityServerOrigin] as string;
        }

        /// <summary>
        /// Gets the base path of IdentityServer. Can be used inside of Katana <c>Map</c>ped middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIdentityServerBasePath(this HttpContext context)
        {
            return context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] as string;
        }

        /// <summary>
        /// Gets the public base URL for IdentityServer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIdentityServerBaseUrl(this HttpContext context)
        {
            return context.GetIdentityServerOrigin() + context.GetIdentityServerBasePath();
        }

        /// <summary>
        /// Gets the identity server relative URL.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetIdentityServerRelativeUrl(this HttpContext context, string path)
        {
            if (!path.IsLocalUrl())
            {
                return null;
            }

            if (path.StartsWith("~/")) path = path.Substring(1);
            path = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + path.RemoveLeadingSlash();
            return path;
        }

        /// <summary>
        /// Gets the identity server issuer URI.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static string GetIdentityServerIssuerUri(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

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

        /// <summary>
        /// Gets the identity server user asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static async Task<ClaimsPrincipal> GetIdentityServerUserAsync(this HttpContext context)
        {
            var userSession = context.RequestServices.GetRequiredService<IUserSession>();
            var user = await userSession.GetIdentityServerUserAsync();
            return user;
        }

        internal static async Task<string> GetIdentityServerSignoutFrameCallbackUrlAsync(this HttpContext context, LogoutMessage logoutMessage = null)
        {
            var userSession = context.RequestServices.GetRequiredService<IUserSession>();

            var user = await userSession.GetIdentityServerUserAsync();
            var currentSubId = user?.GetSubjectId();
            
            var sessions = new List<Session>();
            
            // we check that the sub is the same as the current user, since we 
            // are putting the SLO info in the message back to the end session callback page
            if (currentSubId != null)
            {
                var clientIds = await userSession.GetClientListAsync();
                if (currentSubId == logoutMessage?.SubjectId)
                {
                    // merge the two, since the current list might have changed since end session 
                    // endpoint, meaning the user logged into new clients (albeit unlikely)
                    clientIds = clientIds ?? logoutMessage?.ClientIds;
                    clientIds = clientIds.Distinct();
                }

                if (clientIds.Any())
                {
                    sessions.Add(new Session {
                        SubjectId = currentSubId,
                        SessionId = await userSession.GetCurrentSessionIdAsync(),
                        ClientIds = clientIds,
                    });
                }
            }

            // check if we have signout session info, irrespective to the current user
            if (logoutMessage?.SubjectId != null && logoutMessage?.SubjectId != currentSubId)
            {
                if (logoutMessage.ClientIds?.Any() == true)
                {
                    // we have a logout message in the context of a different user, so add it too
                    sessions.Add(new Session
                    {
                        SubjectId = logoutMessage.SubjectId,
                        SessionId = logoutMessage.SessionId,
                        ClientIds = logoutMessage.ClientIds,
                    });
                }
            }

            if (sessions.Any())
            {
                var endSessionMsg = new EndSession()
                {
                    Sessions = sessions
                };
                var msg = new Message<EndSession>(endSessionMsg);

                var endSessionMessageStore = context.RequestServices.GetRequiredService<IMessageStore<EndSession>>();
                var id = await endSessionMessageStore.WriteAsync(msg);

                var signoutIframeUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.EndSessionCallback;
                signoutIframeUrl = signoutIframeUrl.AddQueryString(Constants.UIConstants.DefaultRoutePathParams.EndSessionCallback, id);

                return signoutIframeUrl;
            }

            // no sessions, so nothing to cleanup
            return null;
        }
    }
}