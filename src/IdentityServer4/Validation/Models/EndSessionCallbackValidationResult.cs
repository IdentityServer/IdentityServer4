// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validation result for end session callback requests.
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.ValidationResult" />
    public class EndSessionCallbackValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets the client logout urls.
        /// </summary>
        /// <value>
        /// The client logout urls.
        /// </value>
        public IEnumerable<string> ClientLogoutUrls { get; internal set; }

        /// <summary>
        /// Gets the logout identifier.
        /// </summary>
        /// <value>
        /// The logout identifier.
        /// </value>
        public string LogoutId { get; internal set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }
    }
}