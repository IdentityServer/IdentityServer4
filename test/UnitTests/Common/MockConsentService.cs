using IdentityServer4.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using System.Security.Claims;
using IdentityServer4.Core.Extensions;

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
