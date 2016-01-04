// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Events
{
    /// <summary>
    /// Event details for logout events
    /// </summary>
    public class LogoutDetails : AuthenticationDetails
    {
        /// <summary>
        /// Gets or sets the sign out identifier.
        /// </summary>
        /// <value>
        /// The sign out identifier.
        /// </value>
        public string SignOutId { get; set; }

        /// <summary>
        /// Gets or sets the sign out message.
        /// </summary>
        /// <value>
        /// The sign out message.
        /// </value>
        public SignOutRequest SignOutMessage { get; set; }
    }
}