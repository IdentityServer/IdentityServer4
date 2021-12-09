//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoApiScope : ApiScope, IMongoEntity<MongoApiScope>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoApiScope>, IndexKeysDefinition<MongoApiScope>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool,
//                    Func<IndexKeysDefinitionBuilder<MongoApiScope>, IndexKeysDefinition<MongoApiScope>>>(false, builder => builder.Ascending(a => a.Name))
//            };
//        }
//    }
//}
