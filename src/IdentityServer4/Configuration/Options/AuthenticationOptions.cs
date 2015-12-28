/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.AspNet.Authentication.Cookies;

namespace IdentityServer4.Core.Configuration
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
            SignInMessageThreshold = Constants.SignInMessageThreshold;

            //PrimaryAuthenticationScheme = Constants.PrimaryAuthenticationType;
        }

        // TODO: new
        public string PrimaryAuthenticationScheme { get; set; }
        internal string EffectivePrimaryAuthenticationScheme
        {
            get
            {
                return PrimaryAuthenticationScheme ?? Constants.PrimaryAuthenticationType;
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

        /// <summary>
        /// Gets or sets the limit after which old signin messages are purged.
        /// Defaults to the value defined in <see cref="Constants.SignInMessageThreshold"/> value.
        /// </summary>
        /// <value>
        /// The limit after which old signin messages are purged
        /// </value>
        public int SignInMessageThreshold { get; set; }
    }
}