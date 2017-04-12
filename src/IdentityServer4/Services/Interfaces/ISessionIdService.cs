// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Interface for the session ID service
    /// </summary>
    public interface ISessionIdService
    {
        /// <summary>
        /// Creates the session identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        void CreateSessionId(SignInContext context);

        /// <summary>
        /// Gets the current session identifier.
        /// </summary>
        /// <returns></returns>
        Task<string> GetCurrentSessionIdAsync();

        /// <summary>
        /// Ensures the session cookie.
        /// </summary>
        /// <returns></returns>
        Task EnsureSessionCookieAsync();

        /// <summary>
        /// Gets the name of the cookie.
        /// </summary>
        /// <returns></returns>
        string GetCookieName();

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <returns></returns>
        string GetCookieValue();

        /// <summary>
        /// Removes the cookie.
        /// </summary>
        void RemoveCookie();
    }
}