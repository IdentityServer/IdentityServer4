using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
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

            return new IdentityServerBuilder(services);
        }
    }
}