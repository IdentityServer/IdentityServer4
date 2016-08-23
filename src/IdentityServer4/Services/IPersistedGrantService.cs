// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Implements persisted grant logic
    /// </summary>
    public interface IPersistedGrantService
    {
        Task StoreAuthorizationCodeAsync(string id, AuthorizationCode code);
        Task<AuthorizationCode> GetAuthorizationCodeAsync(string code);
        Task RemoveAuthorizationCodeAsync(string code);
        Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle);
        Task RemoveRefreshTokenAsync(string refreshTokenHandle);
        Task<Token> GetReferenceTokenAsync(string handle);
        Task RemoveReferenceTokenAsync(string handle);
        Task RemoveRefreshTokensAsync(string subjectId, string clientId);
        Task RemoveReferenceTokensAsync(string subjectId, string clientId);
        Task StoreReferenceTokenAsync(string handle, Token token);
        Task StoreRefreshTokenAsync(string handle, RefreshToken refreshToken);
    }
}