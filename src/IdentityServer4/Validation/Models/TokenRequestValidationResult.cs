// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Validation result for token requests
    /// </summary>
    public class TokenRequestValidationResult : ValidationResult
    {
        public TokenRequestValidationResult(ValidatedTokenRequest validatedRequest)
        {
            IsError = false;
            ValidatedRequest = validatedRequest;
        }

        public TokenRequestValidationResult(string error, string errorDescription = null)
        {
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
        }

        public ValidatedTokenRequest ValidatedRequest { get; }
    }
}