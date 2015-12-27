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

using System.Collections.Specialized;

namespace IdentityServer4.Core.ViewModels
{
    /// <summary>
    /// Models the information and mechanisms for allowing a user to return to a client application.
    /// </summary>
    public class ClientReturnInfo
    {
        /// <summary>
        /// The identifier for the client application the user will be sent to.
        /// </summary>
        /// <value>
        /// The Client Id.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// The name of the client the user will be sent to.
        /// </summary>
        /// <value>
        /// The client name.
        /// </value>
        public string ClientName { get; set; }

        /// <summary>
        /// The Uri of the client where the user can be returned.
        /// </summary>
        /// <value>
        /// The return Uri.
        /// </value>
        public string Uri { get; set; }

        /// <summary>
        /// The HTML-encoded values for the POST body to be used if IsPost is true. 
        /// </summary>
        /// <value>
        /// The POST body.
        /// </value>
        public string PostBody { get; set; }

        /// <summary>
        /// Value that indicates if the return must be performed via a POST, rather than a redirect with GET.
        /// </summary>
        /// <value>
        /// The IsPost flag.
        /// </value>
        public bool IsPost
        {
            get
            {
                return PostBody != null;
            }
        }
    }
}
