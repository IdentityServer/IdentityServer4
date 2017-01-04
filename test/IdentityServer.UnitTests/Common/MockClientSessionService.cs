// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.UnitTests.Common
{
    class MockClientSessionService : IClientSessionService
    {
        public List<string> Clients = new List<string>();

        public bool RemoveCookieWasCalled { get; private set; }

        public Task AddClientIdAsync(string clientId)
        {
            Clients.Add(clientId);
            return Task.FromResult(0);
        }

        public Task EnsureClientListCookieAsync(string sid)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<string>> GetClientListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(Clients);
        }

        public IEnumerable<string> GetClientListFromCookie(string sid)
        {
            return Clients;
        }

        public void RemoveCookie(string sid)
        {
            RemoveCookieWasCalled = true;
            Clients.Clear();
        }
    }
}
