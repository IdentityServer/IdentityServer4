// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityModel;

namespace IdentityServer4.Validation
{
    public class NopResouceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly ILogger<NopResouceOwnerPasswordValidator> _logger;

        public NopResouceOwnerPasswordValidator(ILogger<NopResouceOwnerPasswordValidator> logger)
        {
            _logger = logger;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            context.Result = new GrantValidationResult(OidcConstants.TokenErrors.UnsupportedGrantType);

            _logger.LogWarning("Resource owner password credential type not supported. Configure an IResourceOwnerPasswordValidator.");
            return Task.FromResult(0);
        }
    }
}