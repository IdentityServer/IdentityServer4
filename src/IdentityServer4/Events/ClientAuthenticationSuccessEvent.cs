﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    /// <summary>
    /// Event for successful client authentication
    /// </summary>
    /// <seealso cref="IdentityServer4.Events.Event" />
    public class ClientAuthenticationSuccessEvent : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAuthenticationSuccessEvent"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="authenticationMethod">The authentication method.</param>
        public ClientAuthenticationSuccessEvent(string clientId, string authenticationMethod)
            : base(EventCategories.Authentication, 
                  "Client Authentication Success",
                  EventTypes.Success, 
                  EventIds.ClientAuthenticationSuccess)
        {
            ClientId = clientId;
            AuthenticationMethod = authenticationMethod;
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the authentication method.
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string AuthenticationMethod { get; set; }
    }
}
