// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Service to retrieve and update consent.
    /// </summary>
    public interface IConsentService
    {
        /// <summary>
        /// Checks if consent is required.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subject">The user.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns>Boolean if consent is required.</returns>
        Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes);

        /// <summary>
        /// Updates the consent.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        Task UpdateConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes);
    }
}