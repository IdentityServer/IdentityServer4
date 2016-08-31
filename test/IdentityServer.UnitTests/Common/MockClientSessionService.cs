// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace IdentityServer4.UnitTests.Common
{
    class MockClientSessionService : IClientSessionService
    {
        public List<string> Clients = new List<string>();

        public Task AddClientIdAsync(string clientId)
        {
            Clients.Add(clientId);
            return Task.FromResult(0);
        }

        public Task EnsureClientListCookieAsync()
        {
            return Task.FromResult(0);
        }

        public IEnumerable<string> GetClientListFromCookie()
        {
            return Clients;
        }

        public void RemoveCookie()
        {
            Clients.Clear();
        }
    }
}
