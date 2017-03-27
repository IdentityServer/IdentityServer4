// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public class ApiAuthenticationSuccessEvent : Event
    {
        public ApiAuthenticationSuccessEvent(string apiName, string authenticationMethod)
            : base(EventCategories.Authentication, 
                  "API Authentication Success",
                  EventTypes.Success, 
                  EventIds.ApiAuthenticationSuccess)
        {
            ApiName = apiName;
            AuthenticationMethod = authenticationMethod;
        }

        public string ApiName { get; set; }
        public string AuthenticationMethod { get; set; }
    }
}
