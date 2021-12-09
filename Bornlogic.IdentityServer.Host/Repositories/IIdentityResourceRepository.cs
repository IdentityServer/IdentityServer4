using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    public interface IIdentityResourceRepository
    {
        Task<IEnumerable<IdentityResource>> GetAll();
        Task<IEnumerable<IdentityResource>> GetByScopeNames(IEnumerable<string> scopeNames);
    }
}
