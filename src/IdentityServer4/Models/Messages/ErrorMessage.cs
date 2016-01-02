// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the data for the error page.
    /// </summary>
    public class ErrorMessage
    {
        /// <summary>
        /// The display mode passed from the authorization request.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public string DisplayMode { get; set; }

        /// <summary>
        /// The UI locales passed from the authorization request.
        /// </summary>
        /// <value>
        /// The UI locales.
        /// </value>
        public string UiLocales { get; set; }
        
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
