// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the data submitted from the conset page.
    /// </summary>
    public class ConsentRequest
    {
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the scopes requested.
        /// </summary>
        /// <value>
        /// The scopes requested.
        /// </value>
        public string[] ScopesRequested { get; set; }
    }
}
