// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public class UserLoginFailureEvent : Event
    {
        public UserLoginFailureEvent(string username, string error)
            : base(EventCategories.Authentication,
                  "User Login Failure",
                  EventTypes.Failure, 
                  EventIds.UserLoginFailure,
                  error)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
}
