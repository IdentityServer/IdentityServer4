// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default custom token validator
    /// </summary>
    public class DefaultCustomTokenValidator : ICustomTokenValidator
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The user service
        /// </summary>
        protected readonly IProfileService Profile;

        /// <summary>
        /// The client store
        /// </summary>
        protected readonly IClientStore Clients;

        /// <inheritdoc/>
        public virtual Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public virtual Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }
}