using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer4.Services.Default
{
    /// <summary>
    /// Default wrapper service for IDeviceFlowStore, handling key hashing
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IDeviceFlowCodeService" />
    public class DefaultDeviceFlowCodeService : IDeviceFlowCodeService
    {
        private readonly IDeviceFlowStore _store;
        private readonly IHandleGenerationService _handleGenerationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDeviceFlowCodeService"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        public DefaultDeviceFlowCodeService(IDeviceFlowStore store,
            IHandleGenerationService handleGenerationService)
        {
            _store = store;
            _handleGenerationService = handleGenerationService;
        }

        /// <summary>
        /// Stores the device authorization request.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data)
        {
            var deviceCode = await _handleGenerationService.GenerateAsync();

            await _store.StoreDeviceAuthorizationAsync(deviceCode.Sha256(), userCode.Sha256(), data);

            return deviceCode;
        }

        /// <summary>
        /// Finds device authorization by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        public Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            return _store.FindByUserCodeAsync(userCode.Sha256());
        }

        /// <summary>
        /// Finds device authorization by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            return _store.FindByDeviceCodeAsync(deviceCode.Sha256());
        }

        /// <summary>
        /// Updates device authorization, searching by user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            return _store.UpdateByUserCodeAsync(userCode.Sha256(), data);
        }

        /// <summary>
        /// Removes the device authorization, searching by device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns></returns>
        public Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            return _store.RemoveByDeviceCodeAsync(deviceCode.Sha256());
        }
    }
}