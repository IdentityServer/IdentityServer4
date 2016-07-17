// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Builder;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Configures the login and logout views and behavior.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions()
        {
            EnableLocalLogin = true;
            EnableSignOutPrompt = true;
            RequireAuthenticatedUserForSignOutMessage = false;
            //CookieOptions = new CookieOptions();
        }

        // TODO: new
        public string AuthenticationScheme { get; set; }
        internal string EffectiveAuthenticationScheme
        {
            get
            {
                return AuthenticationScheme ?? Constants.DefaultCookieAuthenticationScheme;
            }
        }

        // TODO: maybe change this so we have a list of grant types for metadata endpoint
        /// <summary>
        /// Gets or sets a value indicating whether local login is enabled.
        /// Disabling this setting will not display the username/password form on the login page. This also will disable the resource owner password flow.
        /// Defaults to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if local login is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableLocalLogin { get; set; }

        public CookieAuthenticationOptions CookieAuthenticationOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IdentityServer will show a confirmation page for sign-out.
        /// When a client initiates a sign-out, by default IdentityServer will ask the user for confirmation. This is a mitigation technique against "logout spam".
        /// Defaults to true.
        /// </summary>
        /// <value>
        /// <c>true</c> if sign-out prompt is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSignOutPrompt { get; set; }

        /// <summary>
        /// Indicates if user must be authenticated to accept parameters to end session endpoint. Defaults to false.
        /// </summary>
        /// <value>
        /// <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireAuthenticatedUserForSignOutMessage { get; set; }
    }
}