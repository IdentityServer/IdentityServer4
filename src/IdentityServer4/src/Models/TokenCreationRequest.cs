// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Validation;
using System;
using System.Security.Claims;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models the data to create a token from a validated request.
    /// </summary>
    public class TokenCreationRequest
    {
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets the resources.
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        public Resources Resources { get; set; }

        /// <summary>
        /// Gets or sets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedRequest ValidatedRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include all identity claims].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include all identity claims]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllIdentityClaims { get; set; }

        /// <summary>
        /// Gets or sets the access token to hash.
        /// </summary>
        /// <value>
        /// The access token to hash.
        /// </value>
        public string AccessTokenToHash { get; set; }

        /// <summary>
        /// Gets or sets the authorization code to hash.
        /// </summary>
        /// <value>
        /// The authorization code to hash.
        /// </value>
        public string AuthorizationCodeToHash { get; set; }

        /// <summary>
        /// Gets or sets pre-hashed state
        /// </summary>
        /// <value>
        /// The pre-hashed state
        /// </value>
        public string StateHash { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>
        /// The nonce.
        /// </value>
        public string Nonce { get; set; }

        /// <summary>
        /// Called to validate the <see cref="TokenCreationRequest"/> before it is processed.
        /// </summary>
        public void Validate()
        {
            if (Resources == null) throw new ArgumentNullException(nameof(Resources));
            if (ValidatedRequest == null) throw new ArgumentNullException(nameof(ValidatedRequest));
        }
    }
}