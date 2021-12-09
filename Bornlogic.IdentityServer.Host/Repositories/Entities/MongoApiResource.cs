//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoApiResource : ApiResource, IMongoEntity<MongoApiResource>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApiResource>, IndexKeysDefinition<MongoApiResource>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApiResource>,
//                    IndexKeysDefinition<MongoApiResource>>>(false, builder => builder.Ascending(a => a.Name)),
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApiResource>,
//                    IndexKeysDefinition<MongoApiResource>>>(false, builder => builder.Ascending(a => a.Scopes))
//            };
//        }
//    }
//}
