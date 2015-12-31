// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Models storage of a subject's permissions for clients. 
    /// Provides an abstraction on the type of permission (codes, refresh tokens, access tokens, and consent).
    /// </summary>
    public interface IPermissionsStore
    {
        /// <summary>
        /// Loads all permissions the subject has granted to all clients.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The permissions.</returns>
        Task<IEnumerable<Consent>> LoadAllAsync(string subject);
        
        /// <summary>
        /// Revokes all permissions the subject has given to a client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        Task RevokeAsync(string subject, string client);
    }
}