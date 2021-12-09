//using Bornlogic.Common.Extensions;
//using Bornlogic.IdentityServer.Storage.Models;
//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.Entities
//{
//    internal class MongoPersistedGrant : PersistedGrant, IMongoEntity<MongoPersistedGrant>
//    {
//        [BsonIgnoreIfDefault]
//        [BsonId]
//        public ObjectId Id { get; set; }

//        public MongoPersistedGrant() { }

//        public MongoPersistedGrant(PersistedGrant persistedGrant)
//        {
//            this.FillProperties(persistedGrant);
//        }

//        public IEnumerable<KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoPersistedGrant>, IndexKeysDefinition<MongoPersistedGrant>>>> GetIndexes()
//        {
//            return new[]
//            {
//                new KeyValuePair<bool, Func<IndexKeysDefinitionBuilder<MongoPersistedGrant>,
//                    IndexKeysDefinition<MongoPersistedGrant>>>(false, builder => builder.Ascending(a => a.Key))
//            };
//        }
//    }
//}
