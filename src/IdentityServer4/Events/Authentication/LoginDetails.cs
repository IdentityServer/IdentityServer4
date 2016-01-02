// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Events
{
    /// <summary>
    /// Event details for login events
    /// </summary>
    public class LoginDetails : AuthenticationDetails
    {
        /// <summary>
        /// Gets or sets the sign in identifier.
        /// </summary>
        /// <value>
        /// The sign in identifier.
        /// </value>
        public string SignInId { get; set; }

        /// <summary>
        /// Gets or sets the sign in message.
        /// </summary>
        /// <value>
        /// The sign in message.
        /// </value>
        public SignInRequest SignInRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether login was a partial login.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is a partial login; otherwise, <c>false</c>.
        /// </value>
        public bool PartialLogin { get; set; }      
    }
}