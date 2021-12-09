//using System.Security.Claims;
//using Bornlogic.Common.Repository.Mongo.Contracts;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;
//using Bornlogic.IdentityServer.Tests.Host.Repositories.UpdateBuilder;
//using Bornlogic.Repositories.MongoDB.Entities;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories
//{
//    internal class UserMongoRepository : IUserRepository
//    {
//        private readonly ISharedMongoRepository<MongoApplicationUser> _repository;
//        private readonly IServiceProvider _serviceProvider;

//        public UserMongoRepository(ISharedMongoRepository<MongoApplicationUser> repository, IServiceProvider serviceProvider)
//        {
//            _repository = repository;
//            _serviceProvider = serviceProvider;
//        }

//        public Task Insert(ApplicationUser user)
//        {
//            return _repository.InsertAsync(new MongoApplicationUser(user));
//        }

//        public Task Update(ApplicationUser user)
//        {
//            return _repository.UpsertOneAsync(a => a.UserID == user.Id, new MongoApplicationUser(user));
//        }

//        public Task UpdateClaims(string userID, IEnumerable<Claim> claims)
//        {
//            return _repository.UpdateAsync(a => a.UserID == userID, new []
//            {
//                MongoFieldToUpdate.Build(() => new MongoApplicationUser().Claims, claims.Select(c => new MongoUserClaim(c))),
//                MongoFieldToUpdate.Build(() => new MongoApplicationUser().ClaimHashes, claims?.Select(c => $"{c.Type}_{c.Value}").ToList() ?? new List<string>()),
//            }, new MongoApplicationUserUpdateBuilder());
//        }

//        public Task DeleteByID(string id)
//        {
//            return _repository.DeleteAsync(a => a.UserID == id);
//        }

//        public async Task<ApplicationUser> GetByID(string id)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.UserID == id);

//            return Convert(mongoEntity);
//        }

//        public async Task<IEnumerable<ApplicationUser>> GetByClaim(Claim claim)
//        {
//            var claimHash = $"{claim.Type}_{claim.Value}";

//            var mongoEntities = await _repository.FindManyAsync(a => a.ClaimHashes.Contains(claimHash));

//            return mongoEntities.Select(Convert).ToList();
//        }
        
//        public async Task<ApplicationUser> GetByUserName(string userName)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.UserName == userName);

//            return Convert(mongoEntity);
//        }

//        public async Task<ApplicationUser> GetByUserEmail(string email)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.Email == email);

//            return Convert(mongoEntity);
//        }

//        public async Task<ApplicationUser> GetByNormalizedUserName(string normalizedUserName)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.NormalizedUserName == normalizedUserName);

//            return Convert(mongoEntity);
//        }

//        public async Task<ApplicationUser> GetByNormalizedUserEmail(string normalizedUserEmail)
//        {
//            var mongoEntity = await _repository.FindOneAsync(a => a.NormalizedEmail == normalizedUserEmail);

//            return Convert(mongoEntity);
//        }

//        private ApplicationUser Convert(MongoApplicationUser user)
//        {
//            if (user == null)
//                return default;

//            return new ApplicationUser
//            {
//                PasswordHash = user.PasswordHash,
//                Id = user.UserID,
//                Claims = user.Claims?.Select(claim => new Claim(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer)) ?? Array.Empty<Claim>(),
//                NormalizedEmail = user.NormalizedEmail,
//                UserName = user.UserName,
//                NormalizedUserName = user.NormalizedUserName,
//                EmailConfirmed = user.EmailConfirmed,
//                AccessFailedCount = user.AccessFailedCount,
//                ConcurrencyStamp = user.ConcurrencyStamp,
//                Email = user.Email,
//                EmployeeId = user.EmployeeId,
//                IsEnabled = user.IsEnabled,
//                LockoutEnabled = user.LockoutEnabled,
//                LockoutEnd = user.LockoutEnd,
//                PhoneNumber = user.PhoneNumber,
//                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
//                SecurityStamp = user.SecurityStamp,
//                TwoFactorEnabled = user.TwoFactorEnabled
//            };
//        }
//    }
//}
