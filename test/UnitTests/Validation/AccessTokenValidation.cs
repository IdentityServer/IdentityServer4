// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#if DNXCORE50
using System.IdentityModel.Tokens.Jwt;
#endif

namespace IdentityServer4.Tests.Validation.Tokens
{
    public class AccessTokenValidation : IDisposable
    {
        const string Category = "Access token validation";

        IClientStore _clients = Factory.CreateClientStore();

        static AccessTokenValidation()
        {
#if DNXCORE50
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();
#endif

#if DNX451
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
#endif
        }

        DateTimeOffset now;
        public DateTimeOffset UtcNow
        {
            get
            {
                if (now > DateTimeOffset.MinValue) return now;
                return DateTimeOffset.UtcNow;
            }
        }

        Func<DateTimeOffset> originalNowFunc;
        
        public AccessTokenValidation()
        {
            originalNowFunc = DateTimeOffsetHelper.UtcNowFunc;
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;
        }

        public void Dispose()
        {
            if (originalNowFunc != null)
            {
                DateTimeOffsetHelper.UtcNowFunc = originalNowFunc;
            }
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeFalse();
            result.Claims.Count().Should().Be(8);
            result.Claims.First(c => c.Type == JwtClaimTypes.ClientId).Value.Should().Be("roclient");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_required_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "read");

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_missing_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "missing");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InsufficientScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unknown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Reference_Token_Too_Long()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);
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
            now = DateTimeOffset.UtcNow;

            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 2, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);
            
            now = now.AddMilliseconds(2000);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.ExpiredToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Malformed_JWT_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unk.nown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_JWT_Token()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write"));

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Issuer()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Issuer = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_Too_Long()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateAccessTokenLong(new Client { ClientId = "roclient" }, "valid", 600, 1000, "read", "write"));
            
            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Audience()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Audience = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        // todo
        //[Fact]
        //[Trait("Category", Category)]
        //public async Task Valid_AccessToken_but_User_not_active()
        //{
        //    var mock = new Mock<IUserService>();
        //    mock.Setup(u => u.IsActiveAsync(It.IsAny<IsActiveContext>())).Callback<IsActiveContext>(ctx=>{
        //        ctx.IsActive = false;
        //    }).Returns(Task.FromResult(0));                        

        //    var store = new InMemoryTokenHandleStore();
        //    var validator = Factory.CreateTokenValidator(tokenStore: store, users: mock.Object);

        //    var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "invalid", 600, "read", "write");
        //    var handle = "123";

        //    await store.StoreAsync(handle, token);

        //    var result = await validator.ValidateAccessTokenAsync("123");

        //    result.IsError.Should().BeTrue();
        //}

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_Client_not_active()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "unknown" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
        }
    }
}