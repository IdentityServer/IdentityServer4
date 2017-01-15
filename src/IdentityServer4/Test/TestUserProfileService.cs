// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Test
{
    public class TestUserProfileService : IProfileService
    {
        private readonly ILogger<TestUserProfileService> _logger;
        private readonly TestUserStore _users;

        public TestUserProfileService(TestUserStore users, ILogger<TestUserProfileService> logger)
        {
            _users = users;
            _logger = logger;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            _logger.LogDebug("Get profile called for {subject} from {client} with {claimTypes} because {caller}",
                context.Subject.GetSubjectId(),
                context.Client.ClientName,
                context.RequestedClaimTypes,
                context.Caller);

            if (context.RequestedClaimTypes.Any())
            {
                var user = _users.FindBySubjectId(context.Subject.GetSubjectId());
                context.AddFilteredClaims(user.Claims);
            }

            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;

            return Task.FromResult(0);
        }
    }
}