// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.TokenRequest
{
    public class TokenRequestValidation_DeviceCode_Invalid
    {
        private const string Category = "TokenRequest Validation - DeviceCode - Invalid";

        private readonly IClientStore _clients = Factory.CreateClientStore();

        private readonly DeviceCode deviceCode = new DeviceCode
        {
            ClientId = "device_flow",
            IsAuthorized = true,
            Subject = new IdentityServerUser("bob").CreatePrincipal(),
            IsOpenId = true,
            Lifetime = 300,
            CreationTime = DateTime.UtcNow,
            AuthorizedScopes = new[] {"openid", "profile", "resource"}
        };

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_DeviceCode()
        {
            var client = await _clients.FindClientByIdAsync("device_flow");
            var store = Factory.CreateDeviceCodeStore();

            var validator = Factory.CreateTokenRequestValidator(deviceCodeStore: store);

            var parameters = new NameValueCollection
            {
                {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode}
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidRequest);
        }
        
        [Fact]
        [Trait("Category", Category)]
        public async Task DeviceCode_Too_Long()
        {
            var client = await _clients.FindClientByIdAsync("device_flow");
            var store = Factory.CreateDeviceCodeStore();

            var longCode = "x".Repeat(new IdentityServerOptions().InputLengthRestrictions.AuthorizationCode + 1);
            
            var validator = Factory.CreateTokenRequestValidator(deviceCodeStore: store);

            var parameters = new NameValueCollection
            {
                {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode},
                {"device_code", longCode}
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Grant_For_Client()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = Factory.CreateDeviceCodeStore();
            var handle = await store.StoreDeviceCodeAsync(deviceCode);

            var validator = Factory.CreateTokenRequestValidator(deviceCodeStore: store);

            var parameters = new NameValueCollection
            {
                {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode},
                {"device_code", handle}
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnauthorizedClient);
        }
    }
}