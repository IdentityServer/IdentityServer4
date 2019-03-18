﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
        public string SessionId { get; set; }
        public AuthenticationProperties Properties { get; set; }


        public Task CreateSessionIdAsync(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            CreateSessionIdWasCalled = true;
            User = principal;
            SessionId = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }

        public Task<ClaimsPrincipal> GetUserAsync()
        {
            return Task.FromResult(User);
        }

        Task<string> IUserSession.GetSessionIdAsync()
        {
            return Task.FromResult(SessionId);
        }

        public Task EnsureSessionIdCookieAsync()
        {
            EnsureSessionIdCookieWasCalled = true;
            return Task.CompletedTask;
        }

        public Task RemoveSessionIdCookieAsync()
        {
            RemoveSessionIdCookieWasCalled = true;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetClientListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(Clients);
        }

        public Task AddClientIdAsync(string clientId)
        {
            Clients.Add(clientId);
            return Task.CompletedTask;
        }
    }
}
