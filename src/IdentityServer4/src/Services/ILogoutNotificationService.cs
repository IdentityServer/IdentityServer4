// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
        /// Performs the http back-channel logout request notifications for the collection of clients.
        /// </summary>
        /// <param name="context">The context for the logout notification.</param>
        Task SendBackChannelLogoutNotificationsAsync(LogoutNotificationContext context);
    }
}
