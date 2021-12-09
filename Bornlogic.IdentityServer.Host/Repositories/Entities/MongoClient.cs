//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoClient : Client, IMongoEntity<MongoClient>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoClient>, IndexKeysDefinition<MongoClient>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoClient>, IndexKeysDefinition<MongoClient>>>(
//                    true, builder => builder.Ascending(a => a.ClientId))
//            };
//        }
//    }
//}
