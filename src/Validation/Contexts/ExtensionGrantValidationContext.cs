﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Class describing the extension grant validation context
    /// </summary>
    public class ExtensionGrantValidationContext
    {
        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public ValidatedTokenRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public GrantValidationResult Result { get; set; } = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
    }
}