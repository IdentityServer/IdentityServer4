﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validation result for authorize requests
    /// </summary>
    public class AuthorizeRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeRequestValidationResult"/> class.
        /// </summary>
        public AuthorizeRequestValidationResult()
        {
            ErrorType = ErrorTypes.User;
        }

        /// <summary>
        /// Gets or sets the type of the error (user vs client).
        /// </summary>
        /// <value>
        /// The type of the error.
        /// </value>
        public ErrorTypes ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedAuthorizeRequest ValidatedRequest { get; set; }
    }
}