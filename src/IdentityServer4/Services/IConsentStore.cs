// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Models persisting user consent
    /// </summary>
    /// TODO: does this really need to be IPermissionsStore?
    public interface IConsentStore : IPermissionsStore
    {
        /// <summary>
        /// Loads the subject's prior consent for the client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns>The persisted consent.</returns>
        Task<Consent> LoadAsync(string subject, string client);

        /// <summary>
        /// Persists the subject's consent.
        /// </summary>
        /// <param name="consent">The consent.</param>
        /// <returns></returns>
        Task UpdateAsync(Consent consent);
    }
}