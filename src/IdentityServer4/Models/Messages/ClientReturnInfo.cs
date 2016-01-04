// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Models
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
