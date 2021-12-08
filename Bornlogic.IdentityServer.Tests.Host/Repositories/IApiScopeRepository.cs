using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Tests.Host.Repositories
{
    public interface IApiScopeRepository
    {
        Task<IEnumerable<ApiScope>> GetAll();
    }
}
