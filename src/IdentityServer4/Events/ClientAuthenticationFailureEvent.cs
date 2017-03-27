// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public class ClientAuthenticationFailureEvent : Event
    {
        public ClientAuthenticationFailureEvent(string clientId, string message)
            : base(EventCategories.Authentication, 
                  "Client Authentication Failure",
                  EventTypes.Failure, 
                  EventIds.ClientAuthenticationFailure, 
                  message)
        {
            ClientId = clientId;
        }

        public string ClientId { get; set; }
    }
}
