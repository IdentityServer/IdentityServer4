//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoApplicationUser : IMongoEntity<MongoApplicationUser>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }
        
//        public string UserID { get; set; }
        
//        public string UserName { get; set; }
        
//        public string NormalizedUserName { get; set; }
        
//        public string Email { get; set; }
        
//        public string NormalizedEmail { get; set; }
        
//        public bool EmailConfirmed { get; set; }
        
//        public string PasswordHash { get; set; }
        
//        public string SecurityStamp { get; set; }
        
//        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        
//        public string PhoneNumber { get; set; }
        
//        public bool PhoneNumberConfirmed { get; set; }
        
//        public bool TwoFactorEnabled { get; set; }
        
//        public DateTimeOffset? LockoutEnd { get; set; }

//        public bool LockoutEnabled { get; set; }
        
//        public int AccessFailedCount { get; set; }
//        public bool IsEnabled { get; set; }
//        public string EmployeeId { get; set; }
//        public IEnumerable<MongoUserClaim> Claims { get; set; }

//        public List<string> ClaimHashes { get; set; }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>(true, builder => builder.Text(a => a.UserID)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>(true, builder => builder.Text(a => a.UserName)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>(true, builder => builder.Text(a => a.NormalizedUserName)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>(true, builder => builder.Text(a => a.Email)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>(true, builder => builder.Text(a => a.NormalizedEmail)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApplicationUser>, IndexKeysDefinition<MongoApplicationUser>>>(false, builder => builder.Ascending(a => a.ClaimHashes))
//            };
//        }

//        public MongoApplicationUser() { }

//        public MongoApplicationUser(ApplicationUser applicationUser)
//        {
//            AccessFailedCount = applicationUser.AccessFailedCount;
//            Claims = applicationUser.Claims?.Select(claim => new MongoUserClaim(claim));
//            ConcurrencyStamp = applicationUser.ConcurrencyStamp;
//            Email = applicationUser.Email;
//            EmailConfirmed = applicationUser.EmailConfirmed;
//            EmployeeId = applicationUser.EmployeeId;
//            IsEnabled = applicationUser.IsEnabled;
//            LockoutEnabled = applicationUser.LockoutEnabled;
//            LockoutEnd = applicationUser.LockoutEnd;
//            NormalizedEmail = applicationUser.NormalizedEmail;
//            NormalizedUserName = applicationUser.NormalizedUserName;
//            PasswordHash = applicationUser.PasswordHash;
//            PhoneNumber = applicationUser.PhoneNumber;
//            PhoneNumberConfirmed = applicationUser.PhoneNumberConfirmed;
//            SecurityStamp = applicationUser.SecurityStamp;
//            TwoFactorEnabled = applicationUser.TwoFactorEnabled;
//            UserID = applicationUser.Id;
//            UserName = applicationUser.UserName;

//            ClaimHashes = Claims?.Select(c => $"{c.Type}_{c.Value}").ToList() ?? new List<string>();
//        }
//    }
//}
