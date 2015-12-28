using IdentityServer4.Core;
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
using System.Collections.Generic;

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

            services.AddEndpoints(options.Endpoints);
            services.AddValidators();
            services.AddResponseGenerators();

            services.AddSecretParsers();
            services.AddSecretValidators();

            services.AddInMemoryTransientStores();
            services.AddCoreServices();
            services.AddHostServices();

            return new IdentityServerBuilder(services);
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services, EndpointOptions endpoints)
        {
            var map = new Dictionary<string, Type>();
            if (endpoints.EnableTokenEndpoint)
            {
                map.Add(Constants.RoutePaths.Oidc.Token, typeof(TokenEndpoint));
            }
            if (endpoints.EnableDiscoveryEndpoint)
            {
                map.Add(Constants.RoutePaths.Oidc.DiscoveryConfiguration, typeof(DiscoveryEndpoint));
            }
            if (endpoints.EnableUserInfoEndpoint)
            {
                map.Add(Constants.RoutePaths.Oidc.UserInfo, typeof(UserInfoEndpoint));
            }
            if (endpoints.EnableIntrospectionEndpoint)
            {
                map.Add(Constants.RoutePaths.Oidc.Introspection, typeof(IntrospectionEndpoint));
            }
            if (endpoints.EnableAuthorizeEndpoint)
            {
                map.Add(Constants.RoutePaths.Oidc.Authorize, typeof(AuthorizeEndpoint));
            }

            services.AddInstance<IEndpointRouter>(new EndpointRouter(map));
            foreach (var item in map)
            {
                services.AddTransient(item.Value);
            }

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.TryAddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
            services.TryAddTransient<TokenRequestValidator>();
            services.TryAddTransient<ScopeValidator>();
            services.TryAddTransient<CustomGrantValidator>();
            services.TryAddTransient<ClientSecretValidator>();
            services.TryAddTransient<TokenValidator>();
            services.TryAddTransient<BearerTokenUsageValidator>();
            services.TryAddTransient<ScopeSecretValidator>();
            services.TryAddTransient<IntrospectionRequestValidator>();
            services.TryAddTransient<IRedirectUriValidator, DefaultRedirectUriValidator>();

            return services;
        }

        public static IServiceCollection AddResponseGenerators(this IServiceCollection services)
        {
            services.TryAddTransient<TokenResponseGenerator>();
            services.TryAddTransient<UserInfoResponseGenerator>();
            services.TryAddTransient<IntrospectionResponseGenerator>();
            services.TryAddTransient<IAuthorizeInteractionResponseGenerator, AuthorizeInteractionResponseGenerator>();
            services.TryAddTransient<IAuthorizeResponseGenerator, AuthorizeResponseGenerator>();
            services.TryAddTransient<IAuthorizationResultGenerator, AuthorizationResultGenerator>();

            return services;
        }

        public static IServiceCollection AddSecretParsers(this IServiceCollection services)
        {
            services.TryAddTransient<SecretParser>();
            services.TryAddEnumerable(new List<ServiceDescriptor>
            {
                new ServiceDescriptor(typeof(ISecretParser), typeof(BasicAuthenticationSecretParser), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(ISecretParser), typeof(PostBodySecretParser), ServiceLifetime.Transient),
            });
            
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
            services.TryAddSingleton<IConsentStore, InMemoryConsentStore>();

            return services;
        }

        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.TryAddTransient<IEventService, DefaultEventService>();
            services.TryAddTransient<ICustomRequestValidator, DefaultCustomRequestValidator>();
            services.TryAddTransient<ITokenService, DefaultTokenService>();
            services.TryAddTransient<ITokenSigningService, DefaultTokenSigningService>();
            services.TryAddTransient<IClaimsProvider, DefaultClaimsProvider>();
            services.TryAddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            services.TryAddTransient<ISigningKeyService, DefaultSigningKeyService>();
            services.TryAddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();
            services.TryAddTransient<ILocalizationService, DefaultLocalizationService>();
            services.TryAddTransient<IConsentService, DefaultConsentService>();

            return services;
        }

        public static IServiceCollection AddHostServices(this IServiceCollection services)
        {
            services.TryAddTransient<ClientListCookie>();

            return services;
        }
    }
}