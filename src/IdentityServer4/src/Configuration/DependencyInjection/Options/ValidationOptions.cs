// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// The ValidationOptions contains settings that affect some of the default validation behavior.
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        ///  Collection of URI scheme prefixes that should never be used as custom URI schemes in the redirect_uri passed to tha authorize endpoint.
        /// </summary>
        public ICollection<string> InvalidRedirectUriPrefixes { get; } = new HashSet<string>
        {
            "javascript:",
            "file:",
            "data:",
            "mailto:",
            "ftp:",
            "blob:",
            "about:",
            "ssh:",
            "tel:",
            "view-source:",
            "ws:",
            "wss:"
        };
    }
}