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

        public bool EnsureSessionIdCookieWasCalled { get; set; }
        public bool RemoveSessionIdCookieWasCalled { get; set; }
        public bool CreateSessionIdWasCalled { get; set; }

        public ClaimsPrincipal User { get; set; }
        public AuthenticationProperties Properties { get; set; }
        public string SessionId { get; set; }

        public Task AddClientIdAsync(string clientId)
        {
            Clients.Add(clientId);
            return Task.CompletedTask;
        }

        public void CreateSessionId(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            CreateSessionIdWasCalled = true;
            SessionId = Guid.NewGuid().ToString();
        }

        public void EnsureSessionIdCookie()
        {
            EnsureSessionIdCookieWasCalled = true;
        }

        public Task<IEnumerable<string>> GetClientListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(Clients);
        }

        public void RemoveSessionIdCookie()
        {
            RemoveSessionIdCookieWasCalled = true;
        }

        public void SetCurrentUser(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            User = principal;
            Properties = properties;
        }
    }
}
