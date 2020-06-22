using System.Threading;
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
        public DefaultDeviceFlowCodeService(
            IDeviceFlowStore store,
            IHandleGenerationService handleGenerationService)
        {
            _store = store;
            _handleGenerationService = handleGenerationService;
        }

        /// <inheritdoc/>
        public async Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data, CancellationToken cancellationToken = default)
        {
            var deviceCode = await _handleGenerationService.GenerateAsync();

            await _store.StoreDeviceAuthorizationAsync(deviceCode.Sha256(), userCode.Sha256(), data, cancellationToken);

            return deviceCode;
        }

        /// <inheritdoc/>
        public Task<DeviceCode> FindByUserCodeAsync(string userCode, CancellationToken cancellationToken = default)
        {
            return _store.FindByUserCodeAsync(userCode.Sha256(), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
        {
            return _store.FindByDeviceCodeAsync(deviceCode.Sha256(), cancellationToken);
        }

        /// <inheritdoc/>
        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data, CancellationToken cancellationToken = default)
        {
            return _store.UpdateByUserCodeAsync(userCode.Sha256(), data, cancellationToken);
        }

        /// <inheritdoc/>
        public Task RemoveByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
        {
            return _store.RemoveByDeviceCodeAsync(deviceCode.Sha256(), cancellationToken);
        }
    }
}