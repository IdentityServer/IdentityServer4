using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Wrapper service for IDeviceFlowStore.
    /// </summary>
    public interface IDeviceFlowCodeService
    {
        /// <summary>
        /// Stores the device authorization request.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data);

        /// <summary>
        /// Finds device authorization by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        Task<DeviceCode> FindByUserCodeAsync(string userCode);

        /// <summary>
        /// Finds device authorization by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode);

        /// <summary>
        /// Updates device authorization, searching by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        Task UpdateByUserCodeAsync(string userCode, DeviceCode data);

        /// <summary>
        /// Removes the device authorization, searching by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        Task RemoveByDeviceCodeAsync(string deviceCode);
    }
}