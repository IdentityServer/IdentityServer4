//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoUserLogin : IMongoEntity<MongoUserLogin>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }

//        public string UserID { get; set; }
//        public string ProviderKey { get; set; }
//        public string ProviderName { get; set; }
//        public string ProviderDisplayName { get; set; }
//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoUserLogin>, IndexKeysDefinition<MongoUserLogin>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoUserLogin>, IndexKeysDefinition<MongoUserLogin>>>(false, builder => builder.Ascending(a => a.UserID).Ascending(a => a.ProviderKey).Ascending(a => a.ProviderName)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoUserLogin>, IndexKeysDefinition<MongoUserLogin>>>(false, builder => builder.Ascending(a => a.ProviderKey).Ascending(a => a.ProviderName))
//            };
//        }
//    }
//}
