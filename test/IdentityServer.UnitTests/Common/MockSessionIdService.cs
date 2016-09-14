﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace IdentityServer4.UnitTests.Common
{
    public class MockSessionIdService : ISessionIdService
    {
        public bool RemoveCookieWasCalled { get; private set; }
        public string SessionId { get; set; } = "session_id";

        public Task AddSessionIdAsync(SignInContext context)
        {
            return Task.FromResult(0);
        }

        public Task EnsureSessionCookieAsync()
        {
            return Task.FromResult(0);
        }

        public string GetCookieName()
        {
            return "sessionid";
        }

        public string GetCookieValue()
        {
            return SessionId;
        }

        public Task<string> GetCurrentSessionIdAsync()
        {
            return Task.FromResult(SessionId);
        }

        public void RemoveCookie()
        {
            RemoveCookieWasCalled = true;
            SessionId = null;
        }
    }
}
