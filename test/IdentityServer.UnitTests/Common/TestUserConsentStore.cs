// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.Models;

namespace IdentityServer4.UnitTests.Common
{
    public class TestUserConsentStore : IUserConsentStore
    {
        DefaultUserConsentStore _userConsentStore;
        InMemoryPersistedGrantStore _grantStore = new InMemoryPersistedGrantStore();

        public TestUserConsentStore()
        {
            _userConsentStore = new DefaultUserConsentStore(
               _grantStore,
               new PersistentGrantSerializer(),
               TestLogger.Create<DefaultUserConsentStore>());
        }

        public Task StoreUserConsentAsync(Consent consent)
        {
            return _userConsentStore.StoreUserConsentAsync(consent);
        }

        public Task<Consent> GetUserConsentAsync(string subjectId, string clientId)
        {
            return _userConsentStore.GetUserConsentAsync(subjectId, clientId);
        }

        public Task RemoveUserConsentAsync(string subjectId, string clientId)
        {
            return _userConsentStore.RemoveUserConsentAsync(subjectId, clientId);
        }
    }
}
