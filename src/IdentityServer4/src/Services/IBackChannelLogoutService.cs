﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// The service responsible for performing back-channel logout notification.
    /// </summary>
    public interface IBackChannelLogoutService
    {
        /// <summary>
        /// Performs http back-channel logout notification.
        /// </summary>
        /// <param name="context">The context of the back channel logout notification.</param>
        Task SendLogoutNotificationsAsync(LogoutNotificationContext context);
    }

    
}
