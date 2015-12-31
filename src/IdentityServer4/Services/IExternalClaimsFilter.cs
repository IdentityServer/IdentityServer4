// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Models a mechanism for claims transformation for claims provided from external identity providers.
    /// </summary>
    public interface IExternalClaimsFilter
    {
        /// <summary>
        /// Filters the specified claims from an external identity provider.
        /// </summary>
        /// <param name="provider">The identifier for the external identity provider.</param>
        /// <param name="claims">The incoming claims.</param>
        /// <returns>The transformed claims.</returns>
        IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims);
    }
}