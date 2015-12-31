// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Indicates if a <see cref="ValidationResult"/> is an error to be displayed to the user or returned to the client.
    /// </summary>
    public enum ErrorTypes
    {
        /// <summary>
        /// client error
        /// </summary>
        Client = 0,

        /// <summary>
        /// user error
        /// </summary>
        User = 1
    }
}
