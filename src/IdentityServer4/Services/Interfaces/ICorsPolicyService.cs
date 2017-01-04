﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Service that determines if CORS is allowed.
    /// </summary>
    public interface ICorsPolicyService
    {
        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        Task<bool> IsOriginAllowedAsync(string origin);
    }
}
