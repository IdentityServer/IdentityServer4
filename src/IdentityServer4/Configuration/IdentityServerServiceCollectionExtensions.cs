// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Hosting.Cors;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Services.Default;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Events;

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
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton(options);
            
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

        public static IServiceCollection AddEndpoints(this IServiceCollection services, EndpointsOptions endpoints)
        {
            var map = new Dictionary<string, Type>();
            if (endpoints.EnableTokenEndpoint)
            {
                map.Add(Constants.ProtocolRoutePaths.Token, typeof(TokenEndpoint));
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

        public static IServiceCollection AddCoreValidators(this IServiceCollection services)
        {
            services.AddTransient<ScopeSecretValidator>();
            services.AddTransient<ScopeValidator>();
            services.AddTransient<ExtensionGrantValidator>();
            services.AddTransient<ClientSecretValidator>();
            services.AddTransient<BearerTokenUsageValidator>();
            services.AddTransient<IEndSessionRequestValidator, EndSessionRequestValidator>();

            return services;
        }

        public static IServiceCollection AddPluggableValidators(this IServiceCollection services)
        {
            services.TryAddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
            services.TryAddTransient<ITokenRequestValidator, TokenRequestValidator>();
            services.TryAddTransient<IRedirectUriValidator, StrictRedirectUriValidator>();
            services.TryAddTransient<ITokenValidator, TokenValidator>();
            services.TryAddTransient<IIntrospectionRequestValidator, IntrospectionRequestValidator>();
            services.TryAddTransient<IResourceOwnerPasswordValidator, NopResouceOwnerPasswordValidator>();
            
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
            services.TryAddTransient<ISigningCredentialStore, InMemorySigningCredentialsStore>();
            services.TryAddTransient<IEventService, DefaultEventService>();
            services.TryAddTransient<ICustomRequestValidator, DefaultCustomRequestValidator>();
            services.TryAddTransient<ITokenService, DefaultTokenService>();
            services.TryAddTransient<ITokenCreationService, DefaultTokenCreationService>();
            services.TryAddTransient<IClaimsProvider, DefaultClaimsProvider>();
            services.TryAddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            services.TryAddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();
            services.TryAddTransient<ILocalizationService, DefaultLocalizationService>();
            services.TryAddTransient<IConsentService, DefaultConsentService>();
            services.TryAddTransient<ICorsPolicyService, DefaultCorsPolicyService>();
            services.TryAddTransient<IProfileService, DefaultProfileService>();
            services.TryAddTransient(typeof(IMessageStore<>), typeof(CookieMessageStore<>));
            services.TryAddTransient<EventServiceHelper>();
            
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
    }
}