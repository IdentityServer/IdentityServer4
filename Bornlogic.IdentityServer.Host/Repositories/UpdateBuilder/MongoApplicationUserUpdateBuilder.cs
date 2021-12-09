//using Bornlogic.IdentityServer.Tests.Host.Repositories.Entities;
//using Bornlogic.Repositories.MongoDB;
//using Bornlogic.Repositories.MongoDB.Entities;
//using MongoDB.Driver;

//namespace Bornlogic.IdentityServer.Tests.Host.Repositories.UpdateBuilder
//{
//    internal class MongoApplicationUserUpdateBuilder : MongoUpdateDefinitionBuilder<MongoApplicationUser>
//    {
//        public override UpdateDefinition<MongoApplicationUser> BuildFromCustomType(MongoUpdateDefinitionBuilderProperty mongoUpdateDefinitionBuilderProperty)
//        {
//            if (mongoUpdateDefinitionBuilderProperty.PropertyGenericType == typeof(MongoUserClaim))
//                return Build<MongoUserClaim>(mongoUpdateDefinitionBuilderProperty);

//            return base.BuildFromCustomType(mongoUpdateDefinitionBuilderProperty);
//        }
//    }
//}
