using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    public interface IApiResourceRepository
    {
        Task<IEnumerable<ApiResource>> GetAll();
    }
}
