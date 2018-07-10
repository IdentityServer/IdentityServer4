using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public interface IDeviceFlowInteractionService
    {
        Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode);
    }
}