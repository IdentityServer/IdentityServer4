// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Services.Default;
using IdentityServer4.Stores.InMemory;

namespace UnitTests.Common
{
    public class TestPersistedGrantService : DefaultPersistedGrantService
    {
        public TestPersistedGrantService() : base(new InMemoryPersistedGrantStore(), TestLogger.Create<DefaultPersistedGrantService>())
        {
        }
    }
}
