using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    internal class DefaultDeviceFlowInteractionService : IDeviceFlowInteractionService
    {
        private readonly IUserCodeStore _userCodes;
        private readonly IClientStore _clients;
        private readonly IUserSession _session;
        private readonly IDeviceCodeStore _devices;
        private readonly ILogger<DefaultDeviceFlowInteractionService> _logger;

        public DefaultDeviceFlowInteractionService(
            IUserCodeStore userCodes,
            IClientStore clients,
            IUserSession session,
            IDeviceCodeStore devices,
            ILogger<DefaultDeviceFlowInteractionService> logger)
        {
            _userCodes = userCodes;
            _clients = clients;
            _session = session;
            _devices = devices;
            _logger = logger;
        }

        public async Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode)
        {
            var userAuth = await _userCodes.GetUserCodeAsync(userCode);
            if (userAuth == null) return LogAndReturnError("Invalid user code", "Device authorization failure - user code is invalid");

            var client = await _clients.FindClientByIdAsync(userAuth.ClientId);
            if (client == null) return LogAndReturnError("Invalid client", "Device authorization failure - requesting client is invalid");

            var user = await _session.GetUserAsync();
            if (user == null) return LogAndReturnError("No user present in device flow request", "Device authorization failure - no user found");

            var deviceAuth = await _devices.GetDeviceCodeAsync(userAuth.DeviceCode);
            if (deviceAuth == null) return LogAndReturnError("Invalid device request", "Device authorization failure - no device code found");

            deviceAuth.IsAuthorized = true;
            deviceAuth.AuthorizedScopes = userAuth.RequestedScopes; // TODO: Switch to consented results
            deviceAuth.Subject = user;
            await _devices.StoreAuthorizedDeviceCodeAsync(userAuth.DeviceCode, deviceAuth);

            await _userCodes.RemoveUserCodeAsync(userCode);

            return new DeviceFlowInteractionResult();
        }

        private DeviceFlowInteractionResult LogAndReturnError(string error, string errorDescription = null)
        {
            _logger.LogError(errorDescription);
            return DeviceFlowInteractionResult.Failure(error);
        }
    }
}