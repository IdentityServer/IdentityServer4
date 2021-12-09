//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoIdentityResource : IdentityResource, IMongoEntity<MongoIdentityResource>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoIdentityResource>, IndexKeysDefinition<MongoIdentityResource>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoIdentityResource>,
//                    IndexKeysDefinition<MongoIdentityResource>>>(false, builder => builder.Ascending(a => a.Name))
//            };
//        }
//    }
//}
