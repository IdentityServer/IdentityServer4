
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
