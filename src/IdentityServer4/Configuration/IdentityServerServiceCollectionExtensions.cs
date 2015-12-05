using IdentityServer4.Core.Validation;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction = null)
        {
            var options = new IdentityServerOptions();

            if (setupAction != null)
            {
                setupAction(options);
            }

            // configuration
            services.AddInstance(options);

            // core services (hard coded for now)
            services.AddTransient<IEventService, DefaultEventService>();
            services.AddTransient<ICustomGrantValidator, NopCustomGrantValidator>();
            services.AddTransient<ICustomRequestValidator, DefaultCustomRequestValidator>();
            services.AddTransient<ITokenService, DefaultTokenService>();
            services.AddTransient<ITokenSigningService, DefaultTokenSigningService>();
            services.AddTransient<IAuthorizationCodeStore, InMemoryAuthorizationCodeStore>();
            services.AddTransient<IRefreshTokenStore, InMemoryRefreshTokenStore>();
            services.AddTransient<IClaimsProvider, DefaultClaimsProvider>();
            services.AddTransient<ITokenHandleStore, InMemoryTokenHandleStore>();
            services.AddTransient<IRefreshTokenService, DefaultRefreshTokenService>();

            // secret parsers
            services.AddTransient<ISecretParser, BasicAuthenticationSecretParser>();
            services.AddTransient<ISecretParser, PostBodySecretParser>();

            // secret validators
            services.AddTransient<ISecretValidator, HashedSharedSecretValidator>();

            // endpoints
            services.AddTransient<TokenEndpoint>();

            // validators
            services.AddTransient<TokenRequestValidator>();
            services.AddTransient<ScopeValidator>();
            services.AddTransient<CustomGrantValidator>();
            services.AddTransient<ClientSecretValidator>();

            // response handlers
            services.AddTransient<TokenResponseGenerator>();

            return services;
        }

        public static IServiceCollection AddInMemoryUsers(this IServiceCollection services, List<InMemoryUser> users)
        {
            var userService = new InMemoryUserService(users);
            return services.AddSingleton<IUserService>(prov => userService);
        }

        public static IServiceCollection AddInMemoryClients(this IServiceCollection services, IEnumerable<Client> clients)
        {
            var clientStore = new InMemoryClientStore(clients);
            return services.AddSingleton<IClientStore>(prov => clientStore);
        }

        public static IServiceCollection AddInMemoryScopes(this IServiceCollection services, IEnumerable<Scope> scopes)
        {
            var scopeStore = new InMemoryScopeStore(scopes);
            return services.AddSingleton<IScopeStore>(prov => scopeStore);
        }
    }
}