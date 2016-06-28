using IdentityServer4.Hosting;
using System;
using System.Diagnostics;
using IdentityModel;

namespace IdentityServer4.Events
{
    public class EventServiceHelper
    {
        private readonly IdentityServerContext _context;

        public EventServiceHelper(IdentityServerContext context)
        {
            _context = context;
        }

        public bool CanRaiseEvent<T>(Event<T> evt)
        {
            switch (evt.EventType)
            {
                case EventTypes.Failure:
                    return _context.Options.EventsOptions.RaiseFailureEvents;
                case EventTypes.Information:
                    return _context.Options.EventsOptions.RaiseInformationEvents;
                case EventTypes.Success:
                    return _context.Options.EventsOptions.RaiseSuccessEvents;
                case EventTypes.Error:
                    return _context.Options.EventsOptions.RaiseErrorEvents;
            }

            return false;
        }

        public virtual Event<T> PrepareEvent<T>(Event<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            evt.Context = new EventContext
            {
                ActivityId = _context.HttpContext.TraceIdentifier,
                TimeStamp = DateTimeOffset.UtcNow,
                ProcessId = Process.GetCurrentProcess().Id,
                //MachineName = Environment..MachineName,
                RemoteIpAddress = _context.HttpContext.Connection.RemoteIpAddress.ToString()
            };

            var principal = _context.HttpContext.User;
            if (principal != null && principal.Identity != null)
            {
                var subjectClaim = principal.FindFirst(JwtClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    evt.Context.SubjectId = subjectClaim.Value;
                }
            }

            return evt;
        }
    }
}