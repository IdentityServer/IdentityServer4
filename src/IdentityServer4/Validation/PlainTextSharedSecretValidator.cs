﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates a secret stored in plain text
    /// </summary>
    public class PlainTextSharedSecretValidator : ISecretValidator
    {
        private readonly ILogger _logger;

        public PlainTextSharedSecretValidator(ILogger<PlainTextSharedSecretValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="System.ArgumentException">id or credential is missing.</exception>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type != Constants.ParsedSecretTypes.SharedSecret)
            {
                _logger.LogError("Parsed secret should not be of type: {type}", parsedSecret.Type ?? "null");
                return fail;
            }

            var sharedSecret = parsedSecret.Credential as string;

            if (parsedSecret.Id.IsMissing() || sharedSecret.IsMissing())
            {
                throw new ArgumentException("Id or Credential is missing.");
            }

            foreach (var secret in secrets)
            {
                var secretDescription = string.IsNullOrEmpty(secret.Description) ? "no description" : secret.Description;

                // this validator is only applicable to shared secrets
                if (secret.Type != Constants.SecretTypes.SharedSecret)
                {
                    _logger.LogDebug("Skipping secret: {description}, secret is not of type {type}.", secretDescription, Constants.SecretTypes.SharedSecret);
                    continue;
                }

                // use time constant string comparison
                var isValid = TimeConstantComparer.IsEqual(sharedSecret, secret.Value);

                if (isValid)
                {
                    return success;
                }
            }

            _logger.LogDebug("No matching plain text secret found.");
            return fail;
        }
    }
}