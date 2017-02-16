// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Configures the login and logout views and behavior.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// Calculates the effective authentication scheme - either none is set, and we default to our constant
        /// or it is set to the scheme of the cookie authentication middleware we should use for maintaing the authentication session
        /// </summary>
        internal string EffectiveAuthenticationScheme => AuthenticationScheme ?? IdentityServerConstants.DefaultCookieAuthenticationScheme;

        /// <summary>
        /// Gets or sets the authentication scheme if you have registered a custom cookie authentication middleware.
        /// </summary>
        /// <value>
        /// The authentication scheme.
        /// </value>
        public string AuthenticationScheme { get; set; }

        /// <summary>
        /// Sets the cookie lifetime (only effective if the built-in cookie middleware is used)
        /// </summary>
        public TimeSpan CookieLifetime { get; set; } = Constants.DefaultCookieTimeSpan;

        /// <summary>
        /// Specified if the cookie should be sliding or not (only effective if the built-in cookie middleware is used)
        /// </summary>
        public bool CookieSlidingExpiration { get; set; } = false;

        /// <summary>
        /// Indicates if user must be authenticated to accept parameters to end session endpoint. Defaults to false.
        /// </summary>
        /// <value>
        /// <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireAuthenticatedUserForSignOutMessage { get; set; } = false;

        /// <summary>
        /// Gets or sets the federated sign out paths.
        /// </summary>
        /// <value>
        /// The federated sign out paths.
        /// </value>
        public ICollection<PathString> FederatedSignOutPaths { get; set; } = new List<PathString>();
    }
}