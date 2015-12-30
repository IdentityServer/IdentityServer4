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
 
using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the data for the error page.
    /// </summary>
    public class ErrorMessage : Message
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// The per-request identifier. This can be used to display to the end user and can be used in diagnostics.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string RequestId { get; set; }

        /// <summary>
        /// Information about the client where a user is allowed to navigate after the error page is displayed.
        /// </summary>
        /// <value>
        /// The return information.
        /// </value>
        public ClientReturnInfo ReturnInfo { get; set; }
    }
}
