//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class IdentityResourceMongoRepository : IIdentityResourceRepository
//    {
//        private readonly ISharedMongoRepository<MongoIdentityResource> _repository;

//        public IdentityResourceMongoRepository(ISharedMongoRepository<MongoIdentityResource> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<IEnumerable<IdentityResource>> GetAll()
//        {
//            var mongoEntries = await _repository.FindManyAsync(a => true);

//            return mongoEntries;
//        }

//        public async Task<IEnumerable<IdentityResource>> GetByScopeNames(IEnumerable<string> scopeNames)
//        {
//            var scopeNamesAsLit = scopeNames.ToList();
            
//            var mongoEntries = await _repository.FindManyAsync(a => scopeNamesAsLit.Contains(a.Name));

//            return mongoEntries;
//        }
//    }
//}
