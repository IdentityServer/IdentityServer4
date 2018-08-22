// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for the device flow store
    /// </summary>
    public interface IDeviceFlowStore
    {
        /// <summary>
        /// Stores the device authorization asynchronous.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data);

        /// <summary>
        /// Finds the by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        Task<DeviceCode> FindByUserCodeAsync(string userCode);

        /// <summary>
        /// Finds the by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode);

        /// <summary>
        /// Updates the by user code asynchronous.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task UpdateByUserCodeAsync(string userCode, DeviceCode data);

        /// <summary>
        /// Removes the by device code asynchronous.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        Task RemoveByDeviceCodeAsync(string deviceCode);
    }
}