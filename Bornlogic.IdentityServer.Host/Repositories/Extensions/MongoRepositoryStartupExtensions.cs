namespace Bornlogic.IdentityServer.Host.Repositories.Extensions
{
    public static class MongoRepositoryStartupExtensions
    {
        public static IServiceCollection RegisterMongoStoreRepositories(this IServiceCollection services)
        {
            //services.AddTransient<IApiResourceRepository, ApiResourceMongoRepository>();
            //services.AddTransient<IApiScopeRepository, ApiScopeMongoRepository>();
            //services.AddTransient<IClientRepository, ClientMongoRepository>();
            //services.AddTransient<IIdentityResourceRepository, IdentityResourceMongoRepository>();
            //services.AddTransient<IPersistentGrantRepository, PersistentGrantMongoRepository>();
            //services.AddTransient<IUserLoginsRepository, UserLoginsMongoRepository>();
            //services.AddTransient<IUserRepository, UserMongoRepository>();
            //services.AddTransient<IUserTokensRepository, UserTokensMongoRepository>();

            return services;
        }
    }
}
