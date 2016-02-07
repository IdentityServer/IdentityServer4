// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints;
using IdentityServer4.Core.Endpoints.Results;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Hosting.Cors;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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

            return services.AddIdentityServer(options);
        }

        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IdentityServerOptions options)
        {
            services.AddInstance(options);

            services.AddAuthentication();

            services.AddTransient<IdentityServerContext>();

            services.AddEndpoints(options.Endpoints);
            services.AddCoreValidators();
            services.AddPluggableValidators();
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

        public static IServiceCollection AddCoreValidators(this IServiceCollection services)
        {
            services.AddTransient<ScopeSecretValidator>();
            services.AddTransient<ScopeValidator>();
            services.AddTransient<CustomGrantValidator>();
            services.AddTransient<ClientSecretValidator>();
            services.AddTransient<BearerTokenUsageValidator>();

            return services;
        }

        public static IServiceCollection AddPluggableValidators(this IServiceCollection services)
        {
            services.TryAddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
            services.TryAddTransient<ITokenRequestValidator, TokenRequestValidator>();
            services.TryAddTransient<IRedirectUriValidator, StrictRedirectUriValidator>();
            services.TryAddTransient<ITokenValidator, TokenValidator>();
            services.TryAddTransient<IIntrospectionRequestValidator, IntrospectionRequestValidator>();

            // todo services.TryAddTransient<IResourceOwnerPasswordValidator, DefaultResouceOwnerPasswordValidator>();
            
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

        public static IServiceCollection AddSecretParsers(this IServiceCollection services)
        {
            services.AddTransient<SecretParser>();
            services.AddTransient<ISecretParser, BasicAuthenticationSecretParser>();
            services.AddTransient<ISecretParser, PostBodySecretParser>();
            
            return services;
        }

        public static IServiceCollection AddSecretValidators(this IServiceCollection services)
        {
            services.AddTransient<SecretValidator>();
            services.AddTransient<ISecretValidator, HashedSharedSecretValidator>();

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
            services.TryAddTransient<ICorsPolicyService, DefaultCorsPolicyService>();
            services.TryAddTransient(typeof(IMessageStore<>), typeof(CookieMessageStore<>));

            return services;
        }

        public static IServiceCollection AddHostServices(this IServiceCollection services)
        {
            services.TryAddTransient<SessionCookie>();
            services.TryAddTransient<ClientListCookie>();
            services.TryAddTransient(typeof(MessageCookie<>));

            services.TryAddTransient<SignInInteraction>();
            services.TryAddTransient<SignOutInteraction>();
            services.TryAddTransient<ConsentInteraction>();
            services.TryAddTransient<ErrorInteraction>();

            services.AddTransient<ICorsPolicyProvider>(provider=>
            {
                return new PolicyProvider(
                    provider.GetRequiredService<ILogger<PolicyProvider>>(),
                    Constants.RoutePaths.CorsPaths,
                    provider.GetRequiredService<ICorsPolicyService>());
            });
            services.AddCors();

            return services;
        }
    }
}