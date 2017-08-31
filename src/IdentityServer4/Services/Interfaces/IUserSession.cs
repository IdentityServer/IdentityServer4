﻿using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Models a user's authentication session
    /// </summary>
    public interface IUserSession
    {
        /// <summary>
        /// Creates a session identifier for the signin context and issues the session id cookie.
        /// </summary>
        void CreateSessionId(ClaimsPrincipal principal, AuthenticationProperties properties);

        /// <summary>
        /// Sets the current user and properties.
        /// </summary>
        /// <returns></returns>
        void SetCurrentUser(ClaimsPrincipal principal, AuthenticationProperties properties);

        /// <summary>
        /// Gets the current authenticated user.
        /// </summary>
        ClaimsPrincipal User { get; }
        
        /// <summary>
        /// Gets the current session identifier.
        /// </summary>
        /// <returns></returns>
        string SessionId { get; }

        /// <summary>
        /// Ensures the session identifier cookie asynchronous.
        /// </summary>
        /// <returns></returns>
        void EnsureSessionIdCookie();

        /// <summary>
        /// Removes the session identifier cookie.
        /// </summary>
        void RemoveSessionIdCookie();

        /// <summary>
        /// Adds a client to the list of clients the user has signed into during their session.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        Task AddClientIdAsync(string clientId);

        /// <summary>
        /// Gets the list of clients the user has signed into during their session.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetClientListAsync();
    }
}
