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
                aggregateCustomValidator = new CustomGrantValidator(new [] { new TestGrantValidator() });
            }
            else
            {
                aggregateCustomValidator = new CustomGrantValidator(customGrantValidators);
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

        //public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
        //    IdentityServerOptions options = null,
        //    IScopeStore scopes = null,
        //    IClientStore clients = null,
        //    IUserService users = null,
        //    ICustomRequestValidator customValidator = null,
        //    IRedirectUriValidator uriValidator = null,
        //    ScopeValidator scopeValidator = null,
        //    IDictionary<string, object> environment = null)
        //{
        //    if (options == null)
        //    {
        //        options = TestIdentityServerOptions.Create();
        //    }

        //    if (scopes == null)
        //    {
        //        scopes = new InMemoryScopeStore(TestScopes.Get());
        //    }

        //    if (clients == null)
        //    {
        //        clients = new InMemoryClientStore(TestClients.Get());
        //    }

        //    if (customValidator == null)
        //    {
        //        customValidator = new DefaultCustomRequestValidator();
        //    }

        //    if (uriValidator == null)
        //    {
        //        uriValidator = new DefaultRedirectUriValidator();
        //    }

        //    if (scopeValidator == null)
        //    {
        //        scopeValidator = new ScopeValidator(scopes);
        //    }

        //    var mockSessionCookie = new Mock<SessionCookie>((IOwinContext)null, (IdentityServerOptions)null);
        //    mockSessionCookie.CallBase = false;
        //    mockSessionCookie.Setup(x => x.GetSessionId()).Returns((string)null);

        //    return new AuthorizeRequestValidator(options, clients, customValidator, uriValidator, scopeValidator, mockSessionCookie.Object);

        //}

        //public static TokenValidator CreateTokenValidator(ITokenHandleStore tokenStore = null, IUserService users = null)
        //{
        //    if (users == null)
        //    {
        //        users = new TestUserService();
        //    }

        //    var clients = CreateClientStore();
        //    var options = TestIdentityServerOptions.Create();
        //    options.Factory = new IdentityServerServiceFactory();
        //    var context = CreateOwinContext(options, clients, users);

        //    var validator = new TokenValidator(
        //        options: options,
        //        clients: clients,
        //        tokenHandles: tokenStore,
        //        customValidator: new DefaultCustomTokenValidator(
        //            users: users,
        //            clients: clients),
        //        owinEnvironment: new OwinEnvironmentService(context));

        //    return validator;
        //}

        //public static IOwinContext CreateOwinContext(IdentityServerOptions options, IClientStore clients, IUserService users)
        //{
        //    options.Factory = options.Factory ?? new IdentityServerServiceFactory();
        //    if (users != null)
        //    {
        //        options.Factory.UserService = new Registration<IUserService>(users);
        //    }
        //    if (options.Factory.UserService == null)
        //    {
        //        options.Factory.UseInMemoryUsers(new List<InMemoryUser>());
        //    }

        //    if (clients != null)
        //    {
        //        options.Factory.ClientStore = new Registration<IClientStore>(clients);
        //    }
        //    if (options.Factory.ClientStore == null)
        //    {
        //        options.Factory.UseInMemoryClients(new List<Client>());
        //    }

        //    if (options.Factory.ScopeStore == null)
        //    {
        //        options.Factory.UseInMemoryScopes(new List<Scope>());
        //    }

        //    var container = AutofacConfig.Configure(options);

        //    var context = new OwinContext();
        //    context.Set(Autofac.Integration.Owin.Constants.OwinLifetimeScopeKey, container);

        //    return context;
        //}
    }
}