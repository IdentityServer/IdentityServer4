//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class ApiScopeMongoRepository : IApiScopeRepository
//    {
//        private readonly ISharedMongoRepository<MongoApiScope> _repository;

//        public ApiScopeMongoRepository(ISharedMongoRepository<MongoApiScope> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<IEnumerable<ApiScope>> GetAll()
//        {
//            var mongoEntities = await _repository.FindManyAsync(a => true);

//            return mongoEntities;
//        }
//    }
//}
