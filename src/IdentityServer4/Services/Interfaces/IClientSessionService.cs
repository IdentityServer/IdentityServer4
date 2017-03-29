// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using System.Collections.Generic;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Interface for the client session service
    /// </summary>
    public interface IClientSessionService
    {
        /// <summary>
        /// Adds a client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        Task AddClientIdAsync(string clientId);

        /// <summary>
        /// Gets the client list.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetClientListAsync();

        /// <summary>
        /// Ensures the client list cookie.
        /// </summary>
        /// <param name="sid">The sid.</param>
        /// <returns></returns>
        Task EnsureClientListCookieAsync(string sid);

        /// <summary>
        /// Gets the client list from the cookie.
        /// </summary>
        /// <param name="sid">The sid.</param>
        /// <returns></returns>
        IEnumerable<string> GetClientListFromCookie(string sid);

        /// <summary>
        /// Removes the cookie.
        /// </summary>
        /// <param name="sid">The sid.</param>
        void RemoveCookie(string sid);
    }
}