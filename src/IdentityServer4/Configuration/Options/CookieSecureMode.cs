// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Configuration
{
    /// <summary>
    /// Indicates if secure flag should be issued for a cookie.
    /// </summary>
    public enum CookieSecureMode
    {
        /// <summary>
        /// The secure flag will be issued if the request is HTTPS.
        /// </summary>
        SameAsRequest = 0,
        /// <summary>
        /// The secure flag will always be issued.
        /// </summary>
        Always = 1,
    }
}
