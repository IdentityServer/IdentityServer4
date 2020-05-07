﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for authorization request messages that are sent from the authorization endpoint to the login and consent UI.
    /// </summary>
    public interface IAuthorizationParametersMessageStore
    {
        /// <summary>
        /// Writes the authorization parameters.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The identifier for the stored message.</returns>
        Task<string> WriteAsync(Message<IDictionary<string, string[]>> message);

        /// <summary>
        /// Reads the authorization parameters.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<Message<IDictionary<string, string[]>>> ReadAsync(string id);

        /// <summary>
        /// Deletes the authorization parameters.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task DeleteAsync(string id);
    }
}