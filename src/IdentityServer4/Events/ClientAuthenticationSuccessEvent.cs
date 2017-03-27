// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public class ClientAuthenticationSuccessEvent : Event
    {
        public ClientAuthenticationSuccessEvent(string clientId, string authenticationMethod)
            : base(EventCategories.Authentication, 
                  "Client Authentication Success",
                  EventTypes.Success, 
                  EventIds.ClientAuthenticationSuccess)
        {
            ClientId = clientId;
            AuthenticationMethod = authenticationMethod;
        }

        public string ClientId { get; set; }
        public string AuthenticationMethod { get; set; }
    }
}
