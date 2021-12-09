//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class ApiResourceMongoRepository : IApiResourceRepository
//    {
//        private readonly ISharedMongoRepository<MongoApiResource> _repository;

//        public ApiResourceMongoRepository(ISharedMongoRepository<MongoApiResource> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<IEnumerable<ApiResource>> GetAll()
//        {
//            var mongoEntries = await _repository.FindManyAsync(a => true);

//            return mongoEntries;
//        }
//    }
//}
