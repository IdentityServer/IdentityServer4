// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Abstraction for storing authorization codes
    /// </summary>
    public interface IAuthorizationCodeStore : ITransientDataRepository<AuthorizationCode>
    { }
}