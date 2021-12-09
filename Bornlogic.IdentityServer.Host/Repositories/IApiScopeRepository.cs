using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    public interface IApiScopeRepository
    {
        Task<IEnumerable<ApiScope>> GetAll();
    }
}
