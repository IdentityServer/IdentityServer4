using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    internal class DefaultDeviceFlowInteractionService : IDeviceFlowInteractionService
    {
        private readonly IClientStore _clients;
        private readonly IUserSession _session;
        private readonly IDeviceFlowStore _devices;
        private readonly ILogger<DefaultDeviceFlowInteractionService> _logger;

        public DefaultDeviceFlowInteractionService(
            IClientStore clients,
            IUserSession session,
            IDeviceFlowStore devices,
            ILogger<DefaultDeviceFlowInteractionService> logger)
        {
            _clients = clients;
            _session = session;
            _devices = devices;
            _logger = logger;
        }

        public async Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode)
        {
            var deviceAuth = await _devices.FindByUserCodeAsync(userCode);
            if (deviceAuth == null) return LogAndReturnError("Invalid user code", "Device authorization failure - user code is invalid");

            var client = await _clients.FindClientByIdAsync(deviceAuth.ClientId);
            if (client == null) return LogAndReturnError("Invalid client", "Device authorization failure - requesting client is invalid");

            var user = await _session.GetUserAsync();
            if (user == null) return LogAndReturnError("No user present in device flow request", "Device authorization failure - no user found");

            deviceAuth.IsAuthorized = true;
            deviceAuth.AuthorizedScopes = deviceAuth.RequestedScopes; // TODO: Switch to consented results
            deviceAuth.Subject = user;
            await _devices.UpdateByUserCodeAsync(userCode, deviceAuth);

            return new DeviceFlowInteractionResult();
        }

        private DeviceFlowInteractionResult LogAndReturnError(string error, string errorDescription = null)
        {
            _logger.LogError(errorDescription);
            return DeviceFlowInteractionResult.Failure(error);
        }
    }
}