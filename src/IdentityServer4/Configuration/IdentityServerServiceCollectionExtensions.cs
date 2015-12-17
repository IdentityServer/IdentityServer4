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
using IdentityServer4.Core.Hosting;

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
            services.AddTransient<IdentityServerContext>();

            // core services (hard coded for now)
            services.AddTransient<IEventService, DefaultEventService>();
            services.AddTransient<ICustomGrantValidator, NopCustomGrantValidator>();
            services.AddTransient<ICustomRequestValidator, DefaultCustomRequestValidator>();
            services.AddTransient<ITokenService, DefaultTokenService>();
            services.AddTransient<ITokenSigningService, DefaultTokenSigningService>();
            services.AddTransient<IClaimsProvider, DefaultClaimsProvider>();
            services.AddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            services.AddTransient<ISigningKeyService, DefaultSigningKeyService>();
            services.AddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();

            // transient stores
            services.AddSingleton<IAuthorizationCodeStore, InMemoryAuthorizationCodeStore>();
            services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
            services.AddSingleton<ITokenHandleStore, InMemoryTokenHandleStore>();
            
            // secret parsers
            services.AddTransient<SecretParser>();
            services.AddTransient<ISecretParser, BasicAuthenticationSecretParser>();
            services.AddTransient<ISecretParser, PostBodySecretParser>();

            // secret validators
            services.AddTransient<SecretValidator>();
            services.AddTransient<ISecretValidator, HashedSharedSecretValidator>();

            // endpoints
            services.AddTransient<TokenEndpoint>();
            services.AddTransient<DiscoveryEndpoint>();
            services.AddTransient<UserInfoEndpoint>();
            services.AddTransient<IntrospectionEndpoint>();

            // validators
            services.AddTransient<TokenRequestValidator>();
            services.AddTransient<ScopeValidator>();
            services.AddTransient<CustomGrantValidator>();
            services.AddTransient<ClientSecretValidator>();
            services.AddTransient<TokenValidator>();
            services.AddTransient<BearerTokenUsageValidator>();
            services.AddTransient<ScopeSecretValidator>();
            services.AddTransient<IntrospectionRequestValidator>();

            // response handlers
            services.AddTransient<TokenResponseGenerator>();
            services.AddTransient<UserInfoResponseGenerator>();
            services.AddTransient<IntrospectionResponseGenerator>();

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