// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.UnitTests.Common
{
    public class MockUserSession : IUserSession
    {
        public List<string> Clients = new List<string>();

        public ClaimsPrincipal User { get; set; }
        public string SessionId { get; set; }

        public bool EnsureSessionIdCookieWasCalled { get; set; }
        public bool RemoveSessionIdCookieWasCalled { get; set; }
        public bool CreateSessionIdWasCalled { get; set; }

        public Task AddClientIdAsync(string clientId)
        {
            Clients.Add(clientId);
            return Task.FromResult(0);
        }

        public void CreateSessionId(AuthenticationProperties properties)
        {
            CreateSessionIdWasCalled = true;
            SessionId = Guid.NewGuid().ToString();
        }

        public Task EnsureSessionIdCookieAsync()
        {
            EnsureSessionIdCookieWasCalled = true;
            return Task.FromResult(0);
        }

        public Task<IEnumerable<string>> GetClientListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(Clients);
        }

        public Task<string> GetCurrentSessionIdAsync()
        {
            return Task.FromResult(SessionId);
        }

        public Task<ClaimsPrincipal> GetIdentityServerUserAsync()
        {
            return Task.FromResult(User);
        }

        public void RemoveSessionIdCookie()
        {
            RemoveSessionIdCookieWasCalled = true;
        }
    }
}
