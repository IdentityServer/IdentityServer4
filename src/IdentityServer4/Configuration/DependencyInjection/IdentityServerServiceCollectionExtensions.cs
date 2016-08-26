// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Events;
using IdentityServer4.Hosting;
using IdentityServer4.Hosting.Cors;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Services.Default;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.Stores.Serialization;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerServiceCollectionExtensions
    {
        //public static IIdentityServerBuilder AddIdentityServerCore(this IServiceCollection services, Action<IdentityServerOptions> setupAction)
        //{
        //    services.AddOptions();
        //    services.Configure(setupAction);

        //    var options = new IdentityServerOptions();
        //    setupAction(options);

        //    services.AddRequiredPlatformServices();
        //    services.AddRequiredServices(options);

        //    return new IdentityServerBuilder(services);
        //}



        //public static IServiceCollection AddRequiredServices(this IServiceCollection services, IdentityServerOptions options)
        //{
        //    services.AddTransient<IdentityServerContext>();
        //    services.AddEndpoints(options.Endpoints);
        //    services.AddValidators();


        //    services.AddResponseGenerators();

        //    services.AddSecretParsers();
        //    services.AddSecretValidators();

        //    services.AddCoreServices();
        //    services.AddHostServices();

        //    return services;
        //}

        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services)
        {
            services.AddSingleton(resolver =>
            {
                return resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value;
            });

            services.AddRequiredPlatformServices();

            services.AddCoreServices();
            services.AddEndpoints();
            services.AddHostServices();
            services.AddPluggableServices();
            services.AddValidators();
            services.AddResponseGenerators();

            services.AddDefaultSecretParsers();
            services.AddDefaultSecretValidators();

            services.AddInMemoryTransientStores();

            return new IdentityServerBuilder(services);
        }

        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityServer();
        }

        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentityServerOptions>(configuration);
            return services.AddIdentityServer();
        }

        public static IServiceCollection AddRequiredPlatformServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAuthentication();
            services.AddOptions();

            return services;
        }

        public static IServiceCollection AddEndpoint<T>(this IServiceCollection services, EndpointName endpoint)
            where T : class, IEndpoint
        {
            services.AddTransient<T>();
            services.AddSingleton(new EndpointMapping { Endpoint = endpoint, Handler = typeof(T) });

            return services;
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddSingleton<IEndpointRouter>(resolver=>
            {
                return new EndpointRouter(Constants.EndpointPathToNameMap,
                    resolver.GetRequiredService<IdentityServerOptions>(), 
                    resolver.GetServices<EndpointMapping>(),
                    resolver.GetRequiredService<ILogger<EndpointRouter>>());
            });
            services.AddEndpoint<AuthorizeEndpoint>(EndpointName.Authorize);
            services.AddEndpoint<CheckSessionEndpoint>(EndpointName.CheckSession);
            services.AddEndpoint<DiscoveryEndpoint>(EndpointName.Discovery);
            services.AddEndpoint<EndSessionEndpoint>(EndpointName.EndSession);
            services.AddEndpoint<IntrospectionEndpoint>(EndpointName.Introspection);
            services.AddEndpoint<RevocationEndpoint>(EndpointName.Revocation);
            services.AddEndpoint<TokenEndpoint>(EndpointName.Token);
            services.AddEndpoint<UserInfoEndpoint>(EndpointName.UserInfo);

            return services;
        }

        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddTransient<ScopeSecretValidator>();
            services.AddTransient<SecretParser>();
            services.AddTransient<ClientSecretValidator>();
            services.AddTransient<SecretValidator>();
            services.AddTransient<ScopeValidator>();
            services.AddTransient<ExtensionGrantValidator>();
            services.AddTransient<BearerTokenUsageValidator>();
            services.AddTransient<PersistentGrantSerializer>();
            services.AddTransient<EventServiceHelper>();

            return services;
        }

        public static IServiceCollection AddPluggableServices(this IServiceCollection services)
        {
            services.TryAddTransient<IPersistedGrantService, DefaultPersistedGrantService>();
            services.TryAddTransient<IKeyMaterialService, DefaultKeyMaterialService>();
            services.TryAddTransient<IEventService, DefaultEventService>();
            services.TryAddTransient<ITokenService, DefaultTokenService>();
            services.TryAddTransient<ITokenCreationService, DefaultTokenCreationService>();
            services.TryAddTransient<IClaimsService, DefaultClaimsService>();
            services.TryAddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            services.TryAddTransient<IConsentService, DefaultConsentService>();
            services.TryAddTransient<ICorsPolicyService, DefaultCorsPolicyService>();
            services.TryAddTransient<IProfileService, DefaultProfileService>();
            services.TryAddTransient(typeof(IMessageStore<>), typeof(CookieMessageStore<>));

            return services;
        }

        public static IServiceCollection AddHostServices(this IServiceCollection services)
        {
            services.TryAddTransient<SessionCookie>();
            services.TryAddTransient<ClientListCookie>();
            services.TryAddTransient(typeof(MessageCookie<>));

            services.TryAddTransient<IUserInteractionService, DefaultUserInteractionService>();

            services.AddTransient<ICorsPolicyProvider>(provider =>
            {
                return new PolicyProvider(
                    provider.GetRequiredService<ILogger<PolicyProvider>>(),
                    Constants.ProtocolRoutePaths.CorsPaths,
                    provider.GetRequiredService<ICorsPolicyService>());
            });
            services.AddCors();

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.TryAddTransient<IEndSessionRequestValidator, EndSessionRequestValidator>();
            services.TryAddTransient<ITokenRevocationRequestValidator, TokenRevocationRequestValidator>();
            services.TryAddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
            services.TryAddTransient<ITokenRequestValidator, TokenRequestValidator>();
            services.TryAddTransient<IRedirectUriValidator, StrictRedirectUriValidator>();
            services.TryAddTransient<ITokenValidator, TokenValidator>();
            services.TryAddTransient<IIntrospectionRequestValidator, IntrospectionRequestValidator>();
            services.TryAddTransient<IResourceOwnerPasswordValidator, NotSupportedResouceOwnerPasswordValidator>();
            services.TryAddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();
            services.TryAddTransient<ICustomRequestValidator, DefaultCustomRequestValidator>();
            services.TryAddTransient<ICustomTokenRequestValidator, DefaultCustomTokenRequestValidator>();

            return services;
        }

        public static IServiceCollection AddResponseGenerators(this IServiceCollection services)
        {
            services.TryAddTransient<ITokenResponseGenerator, TokenResponseGenerator>();
            services.TryAddTransient<IUserInfoResponseGenerator, UserInfoResponseGenerator>();
            services.TryAddTransient<IIntrospectionResponseGenerator, IntrospectionResponseGenerator>();
            services.TryAddTransient<IAuthorizeInteractionResponseGenerator, AuthorizeInteractionResponseGenerator>();
            services.TryAddTransient<IAuthorizeResponseGenerator, AuthorizeResponseGenerator>();
            services.TryAddTransient<IAuthorizeEndpointResultFactory, AuthorizeEndpointResultFactory>();

            return services;
        }

        public static IServiceCollection AddDefaultSecretParsers(this IServiceCollection services)
        {
            services.AddTransient<ISecretParser, BasicAuthenticationSecretParser>();
            services.AddTransient<ISecretParser, PostBodySecretParser>();
            
            return services;
        }

        public static IServiceCollection AddDefaultSecretValidators(this IServiceCollection services)
        {
            services.AddTransient<ISecretValidator, HashedSharedSecretValidator>();

            return services;
        }

        public static IServiceCollection AddInMemoryTransientStores(this IServiceCollection services)
        {
            services.TryAddSingleton<IPersistedGrantStore, InMemoryPersistedGrantStore>();
            services.TryAddSingleton<IConsentStore, InMemoryConsentStore>();

            return services;
        }
    }
}