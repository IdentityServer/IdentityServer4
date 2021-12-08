using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Tests.Host.Repositories
{
    public interface IApiResourceRepository
    {
        Task<IEnumerable<ApiResource>> GetAll();
    }
}
