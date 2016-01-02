// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Class describing the context of the external authentication
    /// </summary>
    public class ExternalAuthenticationContext
    {
        /// <summary>
        /// Gets or sets the external identity.
        /// </summary>
        /// <value>
        /// The external identity.
        /// </value>
        public ExternalIdentity ExternalIdentity { get; set; }
        
        /// <summary>
        /// Gets or sets the sign in message.
        /// </summary>
        /// <value>
        /// The sign in message.
        /// </value>
        public SignInRequest SignInRequest { get; set; }

        /// <summary>
        /// Gets or sets the authenticate result.
        /// </summary>
        /// <value>
        /// The authenticate result.
        /// </value>
        public AuthenticateResult AuthenticateResult { get; set; }
    }
}