// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    /// <summary>
    /// Event for failed client authentication
    /// </summary>
    /// <seealso cref="IdentityServer4.Events.Event" />
    public class ClientAuthenticationFailureEvent : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAuthenticationFailureEvent"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="message">The message.</param>
        public ClientAuthenticationFailureEvent(string clientId, string message)
            : base(EventCategories.Authentication, 
                  "Client Authentication Failure",
                  EventTypes.Failure, 
                  EventIds.ClientAuthenticationFailure, 
                  message)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }
    }
}
