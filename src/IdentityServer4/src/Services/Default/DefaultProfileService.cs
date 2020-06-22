// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default profile service implementation.
    /// This implementation sources all claims from the current subject (e.g. the cookie).
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IProfileService" />
    public class DefaultProfileService : IProfileService
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProfileService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultProfileService(ILogger<DefaultProfileService> logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual Task GetProfileDataAsync(ProfileDataRequestContext context, CancellationToken cancellationToken = default)
        {
            context.LogProfileRequest(Logger);
            context.AddRequestedClaims(context.Subject.Claims);
            context.LogIssuedClaims(Logger);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>s
        public virtual Task IsActiveAsync(IsActiveContext context, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("IsActive called from: {caller}", context.Caller);

            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}
