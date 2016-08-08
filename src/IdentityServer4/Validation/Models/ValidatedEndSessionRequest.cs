// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Represents a validated end session (logout) request
    /// </summary>
    public class ValidatedEndSessionRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

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
    }
}