// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models the validated singout context.
    /// </summary>
    public class LogoutMessage
    {
        public LogoutMessage()
        {
        }

        public LogoutMessage(ValidatedEndSessionRequest request)
        {
            if (request != null)
            {
                ClientId = request.Client?.ClientId;
                if (request.PostLogOutUri != null)
                {
                    PostLogoutRedirectUri = request.PostLogOutUri;
                    if (request.State != null)
                    {
                        PostLogoutRedirectUri = PostLogoutRedirectUri.AddQueryString(OidcConstants.EndSessionRequest.State + "=" + request.State);
                    }
                }
            }
        }
        public LogoutMessage(LogoutMessage message)
        {
            ClientId = message?.ClientId;
            PostLogoutRedirectUri = message?.PostLogoutRedirectUri;
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }
        
        /// <summary>
        /// Gets or sets the post logout redirect URI.
        /// </summary>
        /// <value>
        /// The post logout redirect URI.
        /// </value>
        public string PostLogoutRedirectUri { get; set; }
    }

    /// <summary>
    /// Models the request from a client to sign the user out.
    /// </summary>
    public class LogoutRequest : LogoutMessage
    {
        public LogoutRequest(string iframeUrl, LogoutMessage message)
            : base(message)
        {
            SignOutIFrameUrl = iframeUrl;
        }

        /// <summary>
        /// Gets or sets the sign out iframe URL.
        /// </summary>
        /// <value>
        /// The sign out i frame URL.
        /// </value>
        public string SignOutIFrameUrl { get; set; }
    }
}