using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Tests.Host.Repositories
{
    public interface IClientRepository
    {
        Task<Client> GetByID(string id);
    }
}
