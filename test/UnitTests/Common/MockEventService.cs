// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Services;
using System.Threading.Tasks;
using IdentityServer4.Core.Events;

namespace UnitTests.Common
{
    public class FakeEventService : IEventService
    {
        public Task RaiseAsync<T>(Event<T> evt)
        {
            return Task.FromResult(0);
        }
    }
}
