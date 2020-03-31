﻿using IdentityServer4.Services;
using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    class MockClaimsService : IClaimsService
    {
        public List<Claim> IdentityTokenClaims { get; set; } = new List<Claim>();
        public List<Claim> AccessTokenClaims { get; set; } = new List<Claim>();

        public Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, bool includeAllIdentityClaims, ValidatedRequest request)
        {
            return Task.FromResult(IdentityTokenClaims.AsEnumerable());
        }

        public Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, ValidatedRequest request)
        {
            return Task.FromResult(AccessTokenClaims.AsEnumerable());
        }
    }
}
