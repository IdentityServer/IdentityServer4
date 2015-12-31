// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Context describing the is-active check
    /// </summary>
    public class IsActiveContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsActiveContext"/> class.
        /// </summary>
        public IsActiveContext(ClaimsPrincipal subject, Client client)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (client == null) throw new ArgumentNullException("client");

            Subject = subject;
            Client = client;
            
            IsActive = true;
        }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subject is active and can recieve tokens.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the subject is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
    }
}