using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Services
{
    public interface IDeviceFlowInteractionService
    {
        Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode);
    }
}