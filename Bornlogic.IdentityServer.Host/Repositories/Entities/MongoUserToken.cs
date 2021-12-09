//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoUserToken : IMongoEntity<MongoUserToken>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }
        
//        public string UserID { get; set; }
        
//        public string LoginProvider { get; set; }
        
//        public string Name { get; set; }
        
//        public string Value { get; set; }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoUserToken>, IndexKeysDefinition<MongoUserToken>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoUserToken>,
//                    IndexKeysDefinition<MongoUserToken>>>(false,
//                    builder => builder.Ascending(a => a.UserID).Ascending(a => a.LoginProvider).Ascending(a => a.Name))
//            };
//        }
//    }
//}
