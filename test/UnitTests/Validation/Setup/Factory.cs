/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.Default;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http.Internal;

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
            IUserService userService = null,
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

            if (userService == null)
            {
                userService = new TestUserService();
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
                userService, 
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



        public static TokenValidator CreateTokenValidator(ITokenHandleStore tokenStore = null, IUserService users = null)
        {
            if (users == null)
            {
                users = new TestUserService();
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
                    users: users,
                    clients: clients,
                    logger: new Logger<DefaultCustomTokenValidator>(new LoggerFactory())),
                keyService: new DefaultSigningKeyService(options),
                logger: logger,
                context: idsrvContext);

            return validator;
        }
    }
}