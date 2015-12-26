using IdentityServer4.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Events;
using FluentAssertions;

namespace UnitTests.Common
{
    public class MockEventService : IEventService
    {
        Dictionary<Type, object> _events = new Dictionary<Type, object>();

        public Task RaiseAsync<T>(Event<T> evt)
        {
            _events.Add(typeof(Event<T>), evt);
            return Task.FromResult(0);
        }

        public T AssertEventWasRaised<T>()
            where T : class
        {
            _events.ContainsKey(typeof(T)).Should().BeTrue();
            return (T)_events.Where(x => x.Key == typeof(T)).Select(x=>x.Value).First();
        }
    }
}
