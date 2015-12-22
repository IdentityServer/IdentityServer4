using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder, List<InMemoryUser> users)
        {
            var userService = new InMemoryUserService(users);
            builder.Services.AddSingleton<IUserService>(prov => userService);

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            var clientStore = new InMemoryClientStore(clients);
            builder.Services.AddSingleton<IClientStore>(prov => clientStore);

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryScopes(this IIdentityServerBuilder builder, IEnumerable<Scope> scopes)
        {
            var scopeStore = new InMemoryScopeStore(scopes);
            builder.Services.AddSingleton<IScopeStore>(prov => scopeStore);

            return builder;
        }

        public static IIdentityServerBuilder AddCustomGrantValidator<T>(this IIdentityServerBuilder builder)
            where T : class, ICustomGrantValidator
        {
            builder.Services.AddTransient<ICustomGrantValidator, T>();
            
            return builder;
        }
    }
}