// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using System.Threading.Tasks;
using IdentityServer4.Services;

namespace IdentityServer4.Events
{
    public class DefaultEventService : IEventService
    {
        protected readonly IdentityServerOptions Options;
        protected readonly IHttpContextAccessor Context;
        protected readonly IEventSink Sink;

        public DefaultEventService(IdentityServerOptions options, IHttpContextAccessor context, IEventSink sink)
        {
            Options = options;
            Context = context;
            Sink = sink;
        }

        public async Task RaiseAsync(Event evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            if (CanRaiseEvent(evt))
            {
                await PrepareEventAsync(evt);
                await Sink.PersistAsync(evt);
            }
        }

        public bool CanRaiseEventType(EventTypes evtType)
        {
            switch (evtType)
            {
                case EventTypes.Failure:
                    return Options.Events.RaiseFailureEvents;
                case EventTypes.Information:
                    return Options.Events.RaiseInformationEvents;
                case EventTypes.Success:
                    return Options.Events.RaiseSuccessEvents;
                case EventTypes.Error:
                    return Options.Events.RaiseErrorEvents;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual bool CanRaiseEvent(Event evt)
        {
            return CanRaiseEventType(evt.EventType);
        }

        protected virtual async Task PrepareEventAsync(Event evt)
        {
            evt.ActivityId = Context.HttpContext.TraceIdentifier;
            evt.TimeStamp = IdentityServerDateTime.UtcNow;
            evt.ProcessId = Process.GetCurrentProcess().Id;

            if (Context.HttpContext.Connection.LocalIpAddress != null)
            {
                evt.LocalIpAddress = Context.HttpContext.Connection.LocalIpAddress.ToString() + ":" + Context.HttpContext.Connection.LocalPort;
            }
            else
            {
                evt.LocalIpAddress = "unknown";
            }

            if (Context.HttpContext.Connection.RemoteIpAddress != null)
            {
                evt.RemoteIpAddress = Context.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                evt.RemoteIpAddress = "unknown";
            }

            await evt.PrepareAsync();
        }
    }
}