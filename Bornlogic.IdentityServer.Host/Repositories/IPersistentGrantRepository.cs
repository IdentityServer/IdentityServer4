using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    public interface IPersistentGrantRepository
    {
        Task Insert(PersistedGrant persistedGrant);
        Task<PersistedGrant> GetByKey(string key);
        Task<IEnumerable<PersistedGrant>> GetByFilters(PersistedGrantFilter filter);
        Task DeleteByKey(string key);
        Task DeleteByFilters(PersistedGrantFilter filter);
    }
}
