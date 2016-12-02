﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation
{
    public class AccessTokenValidation : IDisposable
    {
        const string Category = "Access token validation";

        IClientStore _clients = Factory.CreateClientStore();

        static AccessTokenValidation()
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        DateTime now;
        public DateTime UtcNow
        {
            get
            {
                if (now > DateTime.MinValue) return now;
                return DateTime.UtcNow;
            }
        }

        Func<DateTime> originalNowFunc;
        
        public AccessTokenValidation()
        {
            originalNowFunc = DateTimeHelper.UtcNowFunc;
            DateTimeHelper.UtcNowFunc = () => UtcNow;
        }

        public void Dispose()
        {
            if (originalNowFunc != null)
            {
                DateTimeHelper.UtcNowFunc = originalNowFunc;
            }
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token()
        {
            var store = Factory.CreateReferenceTokenStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreReferenceTokenAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeFalse();
            result.Claims.Count().Should().Be(8);
            result.Claims.First(c => c.Type == JwtClaimTypes.ClientId).Value.Should().Be("roclient");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_required_Scope()
        {
            var store = Factory.CreateReferenceTokenStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreReferenceTokenAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "read");

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_missing_Scope()
        {
            var store = Factory.CreateReferenceTokenStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreReferenceTokenAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "missing");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InsufficientScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Reference_Token()
        {
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateAccessTokenAsync("unknown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Reference_Token_Too_Long()
        {
            var validator = Factory.CreateTokenValidator();
            var options = new IdentityServerOptions();

            var longToken = "x".Repeat(options.InputLengthRestrictions.TokenHandle + 1);
            var result = await validator.ValidateAccessTokenAsync(longToken);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Reference_Token()
        {
            now = DateTime.UtcNow;

            var store = Factory.CreateReferenceTokenStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 2, "read", "write");
            var handle = "123";

            await store.StoreReferenceTokenAsync(handle, token);
            
            now = now.AddMilliseconds(2000);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.ExpiredToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Malformed_JWT_Token()
        {
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateAccessTokenAsync("unk.nown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_JWT_Token()
        {
            var signer = Factory.CreateDefaultTokenCreator();
            var jwt = await signer.CreateTokenAsync(TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write"));

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Issuer()
        {
            var signer = Factory.CreateDefaultTokenCreator();
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Issuer = "invalid";
            var jwt = await signer.CreateTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_Too_Long()
        {
            var signer = Factory.CreateDefaultTokenCreator();
            var jwt = await signer.CreateTokenAsync(TokenFactory.CreateAccessTokenLong(new Client { ClientId = "roclient" }, "valid", 600, 1000, "read", "write"));
            
            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Audience()
        {
            var signer = Factory.CreateDefaultTokenCreator();
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Audiences.Clear();
            token.Audiences.Add("invalid");
            var jwt = await signer.CreateTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_Client_not_active()
        {
            var store = Factory.CreateReferenceTokenStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "unknown" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreReferenceTokenAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
        }
    }
}