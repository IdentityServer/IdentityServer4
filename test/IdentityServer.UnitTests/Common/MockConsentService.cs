// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using System.Security.Claims;

namespace UnitTests.Common
{
    public class MockConsentService : IConsentService
    {
        public bool RequiresConsentResult { get; set; }

        public Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes)
        {
            return Task.FromResult(RequiresConsentResult);
        }

        public Client ConsentClient { get; set; }
        public ClaimsPrincipal ConsentSubject { get; set; }
        public IEnumerable<string> ConsentScopes { get; set; }

        public Task UpdateConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes)
        {
            ConsentClient = client;
            ConsentSubject = subject;
            ConsentScopes = scopes;

            return Task.FromResult(0);
        }
    }
}
