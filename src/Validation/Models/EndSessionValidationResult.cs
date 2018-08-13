﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validation result for end session requests
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.ValidationResult" />
    public class EndSessionValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedEndSessionRequest ValidatedRequest { get; set; }
    }
}