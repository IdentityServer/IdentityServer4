// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http.Internal;
using UnitTests.Common;

namespace IdentityServer4.Tests.Validation
{
    static class Factory
    {
        public static IClientStore CreateClientStore()
        {
            return new InMemoryClientStore(TestClients.Get());
        }

        public static ScopeValidator CreateScopeValidator(IScopeStore store)
        {
            return new ScopeValidator(store, new LoggerFactory());
        }

        public static TokenRequestValidator CreateTokenRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IRefreshTokenStore refreshTokens = null,
            IResourceOwnerPasswordValidator resourceOwnerValidator = null,
            IProfileService profile = null,
            IEnumerable<ICustomGrantValidator> customGrantValidators = null,
            ICustomRequestValidator customRequestValidator = null,
            ScopeValidator scopeValidator = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeStore(TestScopes.Get());
            }

            if (resourceOwnerValidator == null)
            {
                resourceOwnerValidator = new TestResourceOwnerPasswordValidator();
            }

            if (profile == null)
            {
                profile = new TestProfileService();
            }

            if (customRequestValidator == null)
            {
                customRequestValidator = new DefaultCustomRequestValidator();
            }

            CustomGrantValidator aggregateCustomValidator;
            if (customGrantValidators == null)
            {
                aggregateCustomValidator = new CustomGrantValidator(new [] { new TestGrantValidator() }, new Logger<CustomGrantValidator>(new LoggerFactory()));
            }
            else
            {
                aggregateCustomValidator = new CustomGrantValidator(customGrantValidators, new Logger<CustomGrantValidator>(new LoggerFactory()));
            }
                
            if (refreshTokens == null)
            {
                refreshTokens = new InMemoryRefreshTokenStore();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(scopes, new LoggerFactory());
            }

            return new TokenRequestValidator(
                options, 
                authorizationCodeStore, 
                refreshTokens, 
                resourceOwnerValidator, 
                profile,
                aggregateCustomValidator, 
                customRequestValidator, 
                scopeValidator, 
                new DefaultEventService(new LoggerFactory()),
                new LoggerFactory());
        }

        internal static ITokenSigningService CreateDefaultTokenSigningService()
        {
            return new DefaultTokenSigningService(new DefaultSigningKeyService(TestIdentityServerOptions.Create()));
        }

        public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IClientStore clients = null,
            IProfileService profile = null,
            ICustomRequestValidator customValidator = null,
            IRedirectUriValidator uriValidator = null,
            ScopeValidator scopeValidator = null,
            IDictionary<string, object> environment = null)
        {
            if (options == null)
            {
                options = TestIdentityServerOptions.Create();
            }

            if (scopes == null)
            {
                scopes = new InMemoryScopeStore(TestScopes.Get());
            }

            if (clients == null)
            {
                clients = new InMemoryClientStore(TestClients.Get());
            }

            if (customValidator == null)
            {
                customValidator = new DefaultCustomRequestValidator();
            }

            if (uriValidator == null)
            {
                uriValidator = new StrictRedirectUriValidator();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(scopes, new LoggerFactory());
            }

            var sessionCookie = new SessionCookie(IdentityServerContextHelper.Create(null, options));

            return new AuthorizeRequestValidator(
                options, 
                clients, 
                customValidator, 
                uriValidator, 
                scopeValidator,
                sessionCookie,
                new Logger<AuthorizeRequestValidator>(new LoggerFactory())
            );
        }

        public static TokenValidator CreateTokenValidator(ITokenHandleStore tokenStore = null, IProfileService profile = null)
        {
            if (profile == null)
            {
                profile = new TestProfileService();
            }

            var clients = CreateClientStore();
            var options = TestIdentityServerOptions.Create();

            var accessor = new HttpContextAccessor();
            accessor.HttpContext = new DefaultHttpContext();
            var idsrvContext = new IdentityServerContext(accessor, options);

            var logger = new Logger<TokenValidator>(new LoggerFactory());

            var validator = new TokenValidator(
                options: options,
                clients: clients,
                tokenHandles: tokenStore,
                customValidator: new DefaultCustomTokenValidator(
                    profile: profile,
                    clients: clients,
                    logger: new Logger<DefaultCustomTokenValidator>(new LoggerFactory())),
                keyService: new DefaultSigningKeyService(options),
                logger: logger,
                context: idsrvContext);

            return validator;
        }
    }
}