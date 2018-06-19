// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for the device code store
    /// </summary>
    public interface IDeviceCodeStore
    {
        /// <summary>
        /// Stores the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<string> StoreDeviceCodeAsync(DeviceCode code);

        /// <summary>
        /// Gets the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<DeviceCode> GetDeviceCodeAsync(string code);

        /// <summary>
        /// Removes the device code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task RemoveDeviceCodeAsync(string code);
    }
}