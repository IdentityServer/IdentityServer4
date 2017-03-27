// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.Events
{
    public class UnhandledExceptionEvent : Event
    {
        public UnhandledExceptionEvent(Exception ex)
            : base(EventCategories.Error,
                  "Unhandled Exception",
                  EventTypes.Error, 
                  EventIds.UnhandledException,
                  ex.Message)
        {
            Details = ex.ToString();
        }

        public string Details { get; set; }
    }
}
