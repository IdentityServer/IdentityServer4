﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Provides features for OIDC signout notifications.
    /// </summary>
    public interface ILogoutNotificationService
    {
        /// <summary>
        /// Builds the URLs needed for front-channel logout notification.
        /// </summary>
        /// <param name="context">The context for the logout notification.</param>
        Task<IEnumerable<string>> GetFrontChannelLogoutNotificationsUrlsAsync(LogoutNotificationContext context);

        /// <summary>
        /// Builds the http back-channel logout request data for the collection of clients.
        /// </summary>
        /// <param name="context">The context for the logout notification.</param>
        Task<IEnumerable<BackChannelLogoutRequest>> GetBackChannelLogoutNotificationsAsync(LogoutNotificationContext context);
    }

    /// <summary>
    /// Information necessary to make a back-channel logout request to a client.
    /// </summary>
    public class BackChannelLogoutRequest
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the back channel logout URI.
        /// </summary>
        public string LogoutUri { get; set; }

        /// <summary>
        /// Gets a value indicating whether the session identifier is required.
        /// </summary>
        public bool SessionIdRequired { get; set; }
    }

}
