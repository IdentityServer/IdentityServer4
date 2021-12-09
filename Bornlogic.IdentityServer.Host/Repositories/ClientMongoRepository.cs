//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class ClientMongoRepository : IClientRepository
//    {
//        private readonly ISharedMongoRepository<MongoClient> _repository;

//        public ClientMongoRepository(ISharedMongoRepository<MongoClient> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<Client> GetByID(string id)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.ClientId == id);

//            return mongoEntity;
//        }
//    }
//}
