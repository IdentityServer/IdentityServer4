//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;
//using Microsoft.AspNetCore.Identity;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class UserTokensMongoRepository : IUserTokensRepository
//    {
//        private readonly ISharedMongoRepository<MongoUserToken> _repository;

//        public UserTokensMongoRepository(ISharedMongoRepository<MongoUserToken> repository)
//        {
//            _repository = repository;
//        }
//        public async Task<IdentityUserToken<string>> GetByFilters(string userID, string name, string provider)
//        {
//            var mongoEntry = await _repository.FindOneAsync(a => a.UserID == userID  && a.LoginProvider == provider && a.Name == name);

//            if (mongoEntry == null)
//                return null;

//            return new IdentityUserToken<string>
//            {
//                Value = mongoEntry.Value,
//                Name = mongoEntry.Name,
//                LoginProvider = mongoEntry.LoginProvider,
//                UserId = mongoEntry.UserID
//            };
//        }

//        public Task Insert(IdentityUserToken<string> token)
//        {
//            return _repository.InsertAsync(new MongoUserToken
//            {
//                Value = token.Value,
//                UserID = token.UserId,
//                Name = token.Name,
//                LoginProvider = token.LoginProvider
//            });
//        }

//        public Task DeleteByFilters(string userID, string name, string provider)
//        {
//            return _repository.DeleteAsync(a => a.UserID == userID && a.LoginProvider == provider && a.Name == name);
//        }
//    }
//}
