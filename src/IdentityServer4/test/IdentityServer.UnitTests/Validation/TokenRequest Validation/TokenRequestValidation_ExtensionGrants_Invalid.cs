﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Validation.TokenRequest_Validation
{
    public class TokenRequestValidation_ExtensionGrants_Invalid
    {
        private const string Category = "TokenRequest Validation - Extension Grants - Invalid";

        private IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Extension_Grant_Type_For_Client_Credentials_Client()
        {
            var client = await _clients.FindEnabledClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection
            {
                { OidcConstants.TokenRequest.GrantType, "customGrant" },
                { OidcConstants.TokenRequest.Scope, "resource" }
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Extension_Grant_Type()
        {
            var client = await _clients.FindEnabledClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection
            {
                { OidcConstants.TokenRequest.GrantType, "unknown_grant_type" },
                { OidcConstants.TokenRequest.Scope, "resource" }
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Customer_Error_and_Description_Extension_Grant_Type()
        {
            var client = await _clients.FindEnabledClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator(extensionGrantValidators: new[] { new TestGrantValidator(isInvalid: true, errorDescription: "custom error description") });

            var parameters = new NameValueCollection
            {
                { OidcConstants.TokenRequest.GrantType, "custom_grant" },
                { OidcConstants.TokenRequest.Scope, "resource" }
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
            result.ErrorDescription.Should().Be("custom error description");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task inactive_user_should_fail()
        {
            var client = await _clients.FindEnabledClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator(
                profile: new TestProfileService(shouldBeActive: false));

            var parameters = new NameValueCollection
            {
                { OidcConstants.TokenRequest.GrantType, "custom_grant" },
                { OidcConstants.TokenRequest.Scope, "resource" }
            };

            var result = await validator.ValidateRequestAsync(
                parameters, 
                client.ToValidationResult());

            result.IsError.Should().BeTrue();
        }
    }
}