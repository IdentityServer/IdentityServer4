using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class QuickstartIdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddInMemoryScopes(this IIdentityServerBuilder builder, IEnumerable<Scope> scopes)
        {
            builder.Services.AddSingleton(scopes);
            builder.Services.AddTransient<IScopeStore, InMemoryScopeStore>();

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            builder.Services.AddSingleton(clients);

            builder.Services.AddTransient<IClientStore, InMemoryClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder, IEnumerable<InMemoryUser> users)
        {
            builder.Services.AddSingleton(users);

            builder.Services.AddTransient<IProfileService, InMemoryUserProfileService>();
            builder.Services.AddTransient<IResourceOwnerPasswordValidator, InMemoryUserResourceOwnerPasswordValidator>();

            return builder;
        }

        public static IIdentityServerBuilder AddInMemoryStores(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IPersistedGrantStore, InMemoryPersistedGrantStore>();

            return builder;
        }
    }
}