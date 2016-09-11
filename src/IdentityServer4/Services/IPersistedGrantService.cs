// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Implements persisted grant logic
    /// </summary>
    public interface IPersistedGrantService
    {
        Task StoreAuthorizationCodeAsync(string handle, AuthorizationCode code);
        Task<AuthorizationCode> GetAuthorizationCodeAsync(string code);
        Task RemoveAuthorizationCodeAsync(string code);

        Task StoreRefreshTokenAsync(string handle, RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle);
        Task RemoveRefreshTokenAsync(string refreshTokenHandle);
        Task RemoveRefreshTokensAsync(string subjectId, string clientId);

        Task StoreReferenceTokenAsync(string handle, Token token);
        Task<Token> GetReferenceTokenAsync(string handle);
        Task RemoveReferenceTokenAsync(string handle);
        Task RemoveReferenceTokensAsync(string subjectId, string clientId);

        Task StoreUserConsentAsync(Consent consent);
        Task<Consent> GetUserConsentAsync(string subjectId, string clientId);
        Task RemoveUserConsentAsync(string subjectId, string clientId);

        Task<IEnumerable<Consent>> GetAllGrantsAsync(string subjectId);
        Task RemoveAllGrantsAsync(string subjectId, string clientId);
    }
}