// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.Services.Default;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using IdentityServer4.Hosting;
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
            return new ScopeValidator(store, TestLogger.Create<ScopeValidator>());
        }

        public static TokenRequestValidator CreateTokenRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IAuthorizationCodeStore authorizationCodeStore = null,
            IRefreshTokenStore refreshTokens = null,
            IResourceOwnerPasswordValidator resourceOwnerValidator = null,
            IProfileService profile = null,
            IEnumerable<IExtensionGrantValidator> extensionGrantValidators = null,
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

            ExtensionGrantValidator aggregateExtensionGrantValidator;
            if (extensionGrantValidators == null)
            {
                aggregateExtensionGrantValidator = new ExtensionGrantValidator(new [] { new TestGrantValidator() }, TestLogger.Create<ExtensionGrantValidator>());
            }
            else
            {
                aggregateExtensionGrantValidator = new ExtensionGrantValidator(extensionGrantValidators, TestLogger.Create<ExtensionGrantValidator>());
            }
                
            if (refreshTokens == null)
            {
                refreshTokens = new InMemoryRefreshTokenStore();
            }

            if (scopeValidator == null)
            {
                scopeValidator = new ScopeValidator(scopes, new LoggerFactory().CreateLogger<ScopeValidator>());
            }

            var idsvrContext = IdentityServerContextHelper.Create();

            return new TokenRequestValidator(
                options, 
                authorizationCodeStore, 
                refreshTokens, 
                resourceOwnerValidator, 
                profile,
                aggregateExtensionGrantValidator, 
                customRequestValidator, 
                scopeValidator, 
                new TestEventService(),
                TestLogger.Create<TokenRequestValidator>());
        }

        internal static ITokenCreationService CreateDefaultTokenCreator()
        {
            return new DefaultTokenCreationService(
                new InMemorySigningCredentialsStore(TestCert.LoadSigningCredentials()));
        }

        public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
            IdentityServerOptions options = null,
            IScopeStore scopes = null,
            IClientStore clients = null,
            IProfileService profile = null,
            ICustomRequestValidator customValidator = null,
            IRedirectUriValidator uriValidator = null,
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
                scopeValidator = new ScopeValidator(scopes, new LoggerFactory().CreateLogger<ScopeValidator>());
            }

            var sessionCookie = new SessionCookie(IdentityServerContextHelper.Create(null, options));

            return new AuthorizeRequestValidator(
                options,
                clients,
                customValidator,
                uriValidator,
                scopeValidator,
                sessionCookie,
                TestLogger.Create<AuthorizeRequestValidator>());
        }

        public static TokenValidator CreateTokenValidator(ITokenHandleStore tokenStore = null, IProfileService profile = null)
        {
            if (profile == null)
            {
                profile = new TestProfileService();
            }

            if (tokenStore == null)
            {
                tokenStore = new InMemoryTokenHandleStore();
            }

            var clients = CreateClientStore();
            var idsvrContext = IdentityServerContextHelper.Create();
            var logger = TestLogger.Create<TokenValidator>();

            var validator = new TokenValidator(
                clients: clients,
                tokenHandles: tokenStore,
                customValidator: new DefaultCustomTokenValidator(
                    profile: profile,
                    clients: clients,
                    logger: TestLogger.Create<DefaultCustomTokenValidator>()),
                    keys: new[] { new InMemoryValidationKeysStore(new[] { TestCert.LoadSigningCredentials().Key }) },
                logger: logger,
                context: idsvrContext);

            return validator;
        }

        public static ClientSecretValidator CreateClientSecretValidator(IClientStore clients = null, SecretParser parser = null, SecretValidator validator = null)
        {
            var options = TestIdentityServerOptions.Create();
            if (clients == null) clients = new InMemoryClientStore(TestClients.Get());

            if (parser == null)
            {
                var parsers = new List<ISecretParser>
                {
                    new BasicAuthenticationSecretParser(options, TestLogger.Create<BasicAuthenticationSecretParser>()),
                    new PostBodySecretParser(options, TestLogger.Create<PostBodySecretParser>())
                };

                parser = new SecretParser(parsers, TestLogger.Create<SecretParser>());
            }

            if (validator == null)
            {
                var validators = new List<ISecretValidator>
                {
                    new HashedSharedSecretValidator(TestLogger.Create<HashedSharedSecretValidator>()),
                    new PlainTextSharedSecretValidator(TestLogger.Create<PlainTextSharedSecretValidator>())
                };

                validator = new SecretValidator(validators, TestLogger.Create<SecretValidator>());
            }

            return new ClientSecretValidator(clients, parser, validator, new TestEventService(), TestLogger.Create<ClientSecretValidator>());
        }
    }
}