// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation.TokenRequest
{

    public class TokenRequestValidation_RefreshToken_Invalid
    {
        const string Category = "TokenRequest Validation - RefreshToken - Invalid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Non_existing_RefreshToken()
        {
            var store = new InMemoryRefreshTokenStore();
            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, "nonexistent");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task RefreshTokenTooLong()
        {
            var store = new InMemoryRefreshTokenStore();
            var client = await _clients.FindClientByIdAsync("roclient");
            var options = new IdentityServerOptions();

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);
            var longRefreshToken = "x".Repeat(options.InputLengthRestrictions.RefreshToken + 1);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, longRefreshToken);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_RefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token") { Client = new Client() { ClientId = "roclient" } },
                LifeTime = 10,
                CreationTime = DateTimeOffset.UtcNow.AddSeconds(-15)
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Wrong_Client_Binding_RefreshToken_Request()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Client = new Client
                    {
                        ClientId = "otherclient"
                    },
                    Lifetime = 600,
                    CreationTime = DateTimeOffset.UtcNow
                }
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_has_no_OfflineAccess_Scope_anymore_at_RefreshToken_Request()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Client = new Client
                    {
                        ClientId = "roclient_restricted"
                    },
                },
                LifeTime = 600,
                CreationTime = DateTimeOffset.UtcNow
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient_restricted");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task RefreshToken_Request_with_disabled_User()
        {
            var subjectClaim = new Claim(JwtClaimTypes.Subject, "foo");

            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Claims = new List<Claim> { subjectClaim },
                    Client = new Client() { ClientId = "roclient" }
                },
                LifeTime = 600,
                CreationTime = DateTimeOffset.UtcNow
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store,
                profile: new TestProfileService(shouldBeActive: false));

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidRequest);
        }
    }
}