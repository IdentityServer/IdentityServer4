//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.IdentityServer.Storage.Stores;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class PersistentGrantMongoRepository : IPersistentGrantRepository
//    {
//        private readonly ISharedMongoRepository<MongoPersistedGrant> _repository;

//        public PersistentGrantMongoRepository(ISharedMongoRepository<MongoPersistedGrant> repository)
//        {
//            _repository = repository;
//        }

//        public Task Insert(PersistedGrant persistedGrant)
//        {
//            return _repository.InsertAsync(new MongoPersistedGrant(persistedGrant));
//        }

//        public async Task<PersistedGrant> GetByKey(string key)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.Key == key);

//            return mongoEntity;
//        }

//        public async Task<IEnumerable<PersistedGrant>> GetByFilters(PersistedGrantFilter filter)
//        {
//            var mongoEntities = await _repository.FindManyAsync(a => 
//                                                                     (filter.ClientId == null || a.ClientId == filter.ClientId) &&
//                                                                     (filter.SubjectId == null || a.SubjectId == filter.SubjectId) &&
//                                                                     (filter.SessionId == null || a.SessionId == filter.SessionId) &&
//                                                                     (filter.Type == null || a.Type == filter.Type));

//            return mongoEntities;
//        }

//        public Task DeleteByKey(string key)
//        {
//            return _repository.DeleteAsync(a => a.Key == key);
//        }

//        public Task DeleteByFilters(PersistedGrantFilter filter)
//        {
//            return _repository.DeleteAsync(a =>
//                (filter.ClientId == null || a.ClientId == filter.ClientId) &&
//                (filter.SubjectId == null || a.SubjectId == filter.SubjectId) &&
//                (filter.SessionId == null || a.SessionId == filter.SessionId) &&
//                (filter.Type == null || a.Type == filter.Type));
//        }
//    }
//}
