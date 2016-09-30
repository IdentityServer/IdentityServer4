﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.Secrets
{
    public class SecretValidation
    {
        const string Category = "Secrets - Secret Validator";

        ISecretValidator _hashedSecretValidator = new HashedSharedSecretValidator(new Logger<HashedSharedSecretValidator>(new LoggerFactory()));
        IClientStore _clients = new InMemoryClientStore(ClientValidationTestClients.Get());
        SecretValidator _validator;

        public SecretValidation()
        {
            _validator = new SecretValidator(new[]
            {
                _hashedSecretValidator
            }, new Logger<SecretValidator>(new LoggerFactory()));
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);

            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Credential_Type()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = "invalid"
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();

            secret.Credential = "foobar";
            result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();

            secret.Credential = "quux";
            result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();

            secret.Credential = "notexpired";
            result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "invalid",
                Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Secret()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "expired",
                Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "invalid",
                Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(secret, client.ClientSecrets);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_with_no_Secret_Should_Throw()
        {
            var clientId = "no_secret_client";
            var client = await _clients.FindEnabledClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
            };
            
            Func<Task> act = () => _validator.ValidateAsync(secret, client.ClientSecrets);

            act.ShouldThrow<ArgumentException>();
        }
    }
}