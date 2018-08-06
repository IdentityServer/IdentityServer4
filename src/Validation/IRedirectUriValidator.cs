﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Models the logic when validating redirect and post logout redirect URIs.
    /// </summary>
    public interface IRedirectUriValidator
    {
        /// <summary>
        /// Determines whether a redirect URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> is the URI is valid; <c>false</c> otherwise.</returns>
        Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client);
        
        /// <summary>
        /// Determines whether a post logout URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> is the URI is valid; <c>false</c> otherwise.</returns>
        Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client);
    }
}