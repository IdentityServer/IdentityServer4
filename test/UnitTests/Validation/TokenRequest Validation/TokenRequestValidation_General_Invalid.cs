// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation.TokenRequest
{
    public class TokenRequestValidation_General_Invalid
    {
        IClientStore _clients = new InMemoryClientStore(TestClients.Get());

        [Fact]
        
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public void Parameters_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            Func<Task> act = () => validator.ValidateRequestAsync(null, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public void Client_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            Func<Task> act = () => validator.ValidateRequestAsync(parameters, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public async Task Unknown_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "unknown");
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public async Task Missing_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }
    }
}