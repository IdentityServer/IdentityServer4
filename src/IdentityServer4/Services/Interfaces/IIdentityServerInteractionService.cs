// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    ///  Provide services be used by the user interface to communicate with IdentityServer.
    /// </summary>
    public interface IIdentityServerInteractionService
    {
        /// <summary>
        /// Gets the authorization context asynchronous.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        Task<AuthorizationRequest> GetAuthorizationContextAsync(string returnUrl);

        /// <summary>
        /// Gets the error context asynchronous.
        /// </summary>
        /// <param name="errorId">The error identifier.</param>
        /// <returns></returns>
        Task<ErrorMessage> GetErrorContextAsync(string errorId);

        /// <summary>
        /// Gets the logout context asynchronous.
        /// </summary>
        /// <param name="logoutId">The logout identifier.</param>
        /// <returns></returns>
        Task<LogoutRequest> GetLogoutContextAsync(string logoutId);
        
        /// <summary>
        /// Creates the logout context asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<string> CreateLogoutContextAsync();

        bool IsValidReturnUrl(string returnUrl);
        Task GrantConsentAsync(AuthorizationRequest request, ConsentResponse consent, string subject = null);

        Task<IEnumerable<Consent>> GetAllUserConsentsAsync();
        Task RevokeUserConsentAsync(string clientId);
        Task RevokeTokensForCurrentSessionAsync();
    }
}
