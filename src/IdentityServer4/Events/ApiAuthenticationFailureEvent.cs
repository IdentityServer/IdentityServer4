// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public class ApiAuthenticationFailureEvent : Event
    {
        public ApiAuthenticationFailureEvent(string apiName, string message)
            : base(EventCategories.Authentication, 
                  "API Authentication Failure",
                  EventTypes.Failure, 
                  EventIds.ApiAuthenticationFailure, 
                  message)
        {
            ApiName = apiName;
        }

        public string ApiName { get; set; }
    }
}
