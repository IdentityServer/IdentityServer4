﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates secrets using the registered validators
    /// </summary>
    public class SecretValidator
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<ISecretValidator> _validators;
        private readonly ISystemClock _clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretValidator"/> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="validators">The validators.</param>
        /// <param name="logger">The logger.</param>
        public SecretValidator(ISystemClock clock, IEnumerable<ISecretValidator> validators, ILogger<SecretValidator> logger)
        {
            _clock = clock;
            _validators = validators;
            _logger = logger;
        }

        /// <summary>
        /// Validates the secret.
        /// </summary>
        /// <param name="parsedSecret">The parsed secret.</param>
        /// <param name="secrets">The secrets.</param>
        /// <returns></returns>
        public async Task<SecretValidationResult> ValidateAsync(ParsedSecret parsedSecret, IEnumerable<Secret> secrets)
        {
            var secretsArray = secrets as Secret[] ?? secrets.ToArray();

            var expiredSecrets = secretsArray.Where(s => s.Expiration.HasExpired(_clock.UtcNow.UtcDateTime)).ToList();
            if (expiredSecrets.Any())
            {
                expiredSecrets.ForEach(
                    ex => _logger.LogWarning("Secret [{description}] is expired", ex.Description ?? "no description"));
            }

            var currentSecrets = secretsArray.Where(s => !s.Expiration.HasExpired(_clock.UtcNow.UtcDateTime)).ToArray();

            // see if a registered validator can validate the secret
            foreach (var validator in _validators)
            {
                var secretValidationResult = await validator.ValidateAsync(currentSecrets, parsedSecret);

                if (secretValidationResult.Success)
                {
                    _logger.LogDebug("Secret validator success: {0}", validator.GetType().Name);
                    return secretValidationResult;
                }
            }

            _logger.LogDebug("Secret validators could not validate secret");
            return new SecretValidationResult { Success = false };
        }
    }
}