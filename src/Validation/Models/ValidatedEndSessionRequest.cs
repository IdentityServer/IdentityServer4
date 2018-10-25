﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Represents a validated end session (logout) request
    /// </summary>
    public class ValidatedEndSessionRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated => Client != null;

        /// <summary>
        /// Gets or sets the post-logout URI.
        /// </summary>
        /// <value>
        /// The post-logout URI.
        /// </value>
        public string PostLogOutUri { get; set; }
        
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        ///  Ids of clients known to have an authentication session for user at end session time
        /// </summary>
        public IEnumerable<string> ClientIds { get; set; }
    }
}