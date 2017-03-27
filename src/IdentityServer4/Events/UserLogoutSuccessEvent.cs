// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public class UserLogoutSuccessEvent : Event
    {
        public UserLogoutSuccessEvent(string subjectId, string name)
            : base(EventCategories.Authentication, 
                  "User Logout Success",
                  EventTypes.Success, 
                  EventIds.UserLogoutSuccess)
        {
            SubjectId = subjectId;
            DisplayName = name;
        }

        public string SubjectId { get; set; }
        public string DisplayName { get; set; }
    }
}
