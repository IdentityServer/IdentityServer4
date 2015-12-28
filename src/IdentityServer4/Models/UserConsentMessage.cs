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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the data submitted from the conset page.
    /// </summary>
    public class UserConsentResponseMessage : Message
    {
        /// <summary>
        /// Gets if consent was granted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent was granted; otherwise, <c>false</c>.
        /// </value>
        public bool Granted
        {
            get
            {
                return ScopesConsented != null && ScopesConsented.Any();
            }
        }

        /// <summary>
        /// Gets or sets the scopes consented to.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string[] ScopesConsented { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wishes the consent to be remembered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent is to be remembered; otherwise, <c>false</c>.
        /// </value>
        public bool RememberConsent { get; set; }

        public NameValueCollection RequestParameters { get; set; }
    }

    /// <summary>
    /// Models the data submitted from the conset page.
    /// </summary>
    public class UserConsentRequestMessage : Message
    {
        /// <summary>
        /// Gets or sets the scopes requested.
        /// </summary>
        /// <value>
        /// The scopes requested.
        /// </value>
        public string[] ScopesRequested { get; set; }

        public NameValueCollection RequestParameters { get; set; }
    }
}
