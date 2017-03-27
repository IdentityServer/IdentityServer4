// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default profile service implementation.
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IProfileService" />
    public class DefaultProfileService : IProfileService
    {
        private readonly ILogger<DefaultProfileService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProfileService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultProfileService(ILogger<DefaultProfileService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            _logger.LogDebug("Get profile called for {subject} from {client} with {claimTypes} because {caller}",
                context.Subject.GetSubjectId(),
                context.Client.ClientName,
                context.RequestedClaimTypes,
                context.Caller);

            if (context.RequestedClaimTypes.Any())
            {
                context.AddFilteredClaims(context.Subject.Claims);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }
    }
}