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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

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


        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction = null)
        {
            var options = new IdentityServerOptions();
            setupAction?.Invoke(options);

            return services.AddIdentityServer(options);
        }

        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IdentityServerOptions options)
        {
            services.AddSingleton(options);
            services.AddRequiredPlatformServices();

            services.AddEndpoints(options.Endpoints);
            services.AddCoreServices();
            services.AddHostServices();
            services.AddPluggableServices();
            services.AddValidators();
            services.AddResponseGenerators();

            services.AddDefaultSecretParsers();
            services.AddDefaultSecretValidators();

            services.AddInMemoryTransientStores();
            

            return new IdentityServerBuilder(services);
        }

        public static IServiceCollection AddRequiredPlatformServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAuthentication();

            return services;
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services, EndpointsOptions endpoints)
        {
            var map = new Dictionary<string, Type>();
            if (endpoints.EnableTokenEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.Token, typeof(TokenEndpoint));
            }
            if (endpoints.EnableTokenRevocationEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.Revocation, typeof(RevocationEndpoint));
            }
            if (endpoints.EnableDiscoveryEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.DiscoveryConfiguration, typeof(DiscoveryEndpoint));
            }
            if (endpoints.EnableUserInfoEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.UserInfo, typeof(UserInfoEndpoint));
            }
            if (endpoints.EnableIntrospectionEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.Introspection, typeof(IntrospectionEndpoint));
            }
            if (endpoints.EnableAuthorizeEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.Authorize, typeof(AuthorizeEndpoint));
            }
            if (endpoints.EnableEndSessionEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.EndSession, typeof(EndSessionEndpoint));
            }
            if (endpoints.EnableCheckSessionEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.CheckSession, typeof(CheckSessionEndpoint));
            }

            services.AddSingleton<IEndpointRouter>(new EndpointRouter(map));
            foreach (var item in map)
            {
                services.AddTransient(item.Value);
            }

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