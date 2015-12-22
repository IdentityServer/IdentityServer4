using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerServiceCollectionExtensions
    {
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction = null)
        {
            var options = new IdentityServerOptions();

            if (setupAction != null)
            {
                setupAction(options);
            }

            services.AddInstance(options);
            services.AddTransient<IdentityServerContext>();

            services.AddEndpoints();
            services.AddValidators();
            services.AddResponseGenerators();

            services.AddSecretParsers();
            services.AddSecretValidators();

            services.AddInMemoryTransientStores();
            services.AddCoreServices();
            
            return new IdentityServerBuilder(services);
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.TryAddTransient<TokenEndpoint>();
            services.TryAddTransient<DiscoveryEndpoint>();
            services.TryAddTransient<UserInfoEndpoint>();
            services.TryAddTransient<IntrospectionEndpoint>();

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.TryAddTransient<TokenRequestValidator>();
            services.TryAddTransient<ScopeValidator>();
            services.TryAddTransient<CustomGrantValidator>();
            services.TryAddTransient<ClientSecretValidator>();
            services.TryAddTransient<TokenValidator>();
            services.TryAddTransient<BearerTokenUsageValidator>();
            services.TryAddTransient<ScopeSecretValidator>();
            services.TryAddTransient<IntrospectionRequestValidator>();

            return services;
        }

        public static IServiceCollection AddResponseGenerators(this IServiceCollection services)
        {
            services.TryAddTransient<TokenResponseGenerator>();
            services.TryAddTransient<UserInfoResponseGenerator>();
            services.TryAddTransient<IntrospectionResponseGenerator>();

            return services;
        }

        public static IServiceCollection AddSecretParsers(this IServiceCollection services)
        {
            services.TryAddTransient<SecretParser>();
            services.TryAddTransient<ISecretParser, BasicAuthenticationSecretParser>();
            services.TryAddTransient<ISecretParser, PostBodySecretParser>();

            return services;
        }

        public static IServiceCollection AddSecretValidators(this IServiceCollection services)
        {
            services.TryAddTransient<SecretValidator>();
            services.TryAddTransient<ISecretValidator, HashedSharedSecretValidator>();

            return services;
        }

        public static IServiceCollection AddInMemoryTransientStores(this IServiceCollection services)
        {
            services.TryAddSingleton<IAuthorizationCodeStore, InMemoryAuthorizationCodeStore>();
            services.TryAddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
            services.TryAddSingleton<ITokenHandleStore, InMemoryTokenHandleStore>();

            return services;
        }

        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.TryAddTransient<IEventService, DefaultEventService>();
            services.TryAddTransient<ICustomGrantValidator, NopCustomGrantValidator>();
            services.TryAddTransient<ICustomRequestValidator, DefaultCustomRequestValidator>();
            services.TryAddTransient<ITokenService, DefaultTokenService>();
            services.TryAddTransient<ITokenSigningService, DefaultTokenSigningService>();
            services.TryAddTransient<IClaimsProvider, DefaultClaimsProvider>();
            services.TryAddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            services.TryAddTransient<ISigningKeyService, DefaultSigningKeyService>();
            services.TryAddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();

            return services;
        }
    }
}