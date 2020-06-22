// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for user consent storage
    /// </summary>
    public interface IUserConsentStore
    {
        /// <summary>
        /// Stores the user consent.
        /// </summary>
        /// <param name="consent">The consent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task StoreUserConsentAsync(Consent consent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the user consent.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<Consent> GetUserConsentAsync(string subjectId, string clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the user consent.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task RemoveUserConsentAsync(string subjectId, string clientId, CancellationToken cancellationToken = default);
    }
}