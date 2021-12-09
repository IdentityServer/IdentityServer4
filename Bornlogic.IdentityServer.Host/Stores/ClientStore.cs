using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Tests.Host.Repositories;

namespace Bornlogic.IdentityServer.Tests.Host.Stores
{
    public class ClientStore : IClientStore
    {
        private readonly IClientRepository _clientStoreRepository;

        public ClientStore(IClientRepository clientStoreRepository)
        {
            _clientStoreRepository = clientStoreRepository;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return _clientStoreRepository.GetByID(clientId);
        }
    }
}