﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Refresh token storage
    /// </summary>
    public interface IRefreshTokenStore : ITransientDataRepository<RefreshToken>
    { }
}