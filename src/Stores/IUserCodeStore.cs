// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for the user code store
    /// </summary>
    public interface IUserCodeStore
    {
        /// <summary>
        /// Stores the user code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task StoreUserCodeAsync(string code, UserCode data);

        /// <summary>
        /// Gets the user code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<UserCode> GetUserCodeAsync(string code);

        /// <summary>
        /// Removes the user code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task RemoveUserCodeAsync(string code);
    }
}