﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System.Collections.Generic;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// Models a token error response
    /// </summary>
    public class TokenErrorResponse
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; } = OidcConstants.TokenErrors.InvalidRequest;

        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>
        /// The error description.
        /// </value>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the custom entries.
        /// </summary>
        /// <value>
        /// The custom.
        /// </value>
        public Dictionary<string, object> Custom { get; set; } = new Dictionary<string, object>();
    }
}