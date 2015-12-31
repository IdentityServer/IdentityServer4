// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Logic for signing security tokens
    /// </summary>
    public interface ITokenSigningService
    {
        /// <summary>
        /// Signs the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A protected and serialized security token</returns>
        Task<string> SignTokenAsync(Token token);
    }
}