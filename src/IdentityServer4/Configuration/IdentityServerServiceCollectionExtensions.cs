using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerServiceCollectionExtensions
    {
        private static void AddIdentityServer(IServiceCollection services, Action<IdentityServerOptions> setupAction = null)
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
            //services.AddTransient<IRefreshTokenService, InMemoryRefreshTokenStore>();

            // endpoints
            services.AddTransient<IEndpoint, TokenEndpoint>();

            // validators
            services.AddTransient<TokenRequestValidator>();
            services.AddTransient<ScopeValidator>();

            // response handlers
            services.AddTransient<TokenResponseGenerator>();
        }
    }
}

