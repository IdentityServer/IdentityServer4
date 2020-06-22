using System.Threading;
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <param name="data">The data.</param>
        Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds device authorization by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task<DeviceCode> FindByUserCodeAsync(string userCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds device authorization by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates device authorization, searching by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        Task UpdateByUserCodeAsync(string userCode, DeviceCode data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the device authorization, searching by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        Task RemoveByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default);
    }
}