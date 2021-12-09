//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;
//using Microsoft.AspNetCore.Identity;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class UserLoginsMongoRepository : IUserLoginsRepository
//    {
//        private readonly ISharedMongoRepository<MongoUserLogin> _repository;

//        public UserLoginsMongoRepository(ISharedMongoRepository<MongoUserLogin> repository)
//        {
//            _repository = repository;
//        }

//        public async Task<IEnumerable<UserLoginInfo>> GetByID(string userID)
//        {
//            var mongoEntry = await _repository.FindManyAsync(a => a.UserID == userID);

//            return mongoEntry.Select(login => new UserLoginInfo(login.ProviderName, login.ProviderKey, login.ProviderDisplayName)).ToList();
//        }

//        public async Task<IdentityUserLogin<string>> GetByFilters(string userID, string provider, string providerKey)
//        {
//            var mongoEntry = await _repository.FindOneAsync(a => a.UserID == userID && a.ProviderKey == providerKey && a.ProviderName == provider);

//            if (mongoEntry == null)
//                return null;

//            return new IdentityUserLogin<string>
//            {
//                UserId = mongoEntry.UserID,
//                ProviderKey = mongoEntry.ProviderKey,
//                ProviderDisplayName = mongoEntry.ProviderDisplayName,
//                LoginProvider = mongoEntry.ProviderName
//            };
//        }

//        public async Task<IdentityUserLogin<string>> GetByFilters(string provider, string providerKey)
//        {
//            var mongoEntries = await _repository.FindManyAsync(a => a.ProviderKey == providerKey && a.ProviderName == provider);
            
//            return mongoEntries.Select(mongoEntry => new IdentityUserLogin<string>
//            {
//                UserId = mongoEntry.UserID,
//                ProviderKey = mongoEntry.ProviderKey,
//                ProviderDisplayName = mongoEntry.ProviderDisplayName,
//                LoginProvider = mongoEntry.ProviderName
//            }).ToList().FirstOrDefault();
//        }

//        public Task Insert(string userID, UserLoginInfo login)
//        {
//            return _repository.InsertAsync(new MongoUserLogin
//            {
//                UserID = userID,
//                ProviderName = login.LoginProvider,
//                ProviderKey = login.ProviderKey,
//                ProviderDisplayName = login.ProviderDisplayName
//            });
//        }

//        public Task DeleteByFilters(string userID, string provider, string providerKey)
//        {
//            return _repository.DeleteAsync(a => a.UserID == userID && a.ProviderKey == providerKey && a.ProviderName == provider);
//        }
//    }
//}
