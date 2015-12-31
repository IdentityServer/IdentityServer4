// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Events
{
    /// <summary>
    /// Event details for local login events
    /// </summary>
    public class LocalLoginDetails : LoginDetails
    {
        /// <summary>
        /// Gets or sets the name of the login user.
        /// </summary>
        /// <value>
        /// The name of the login user.
        /// </value>
        public string LoginUserName { get; set; }
    }
}