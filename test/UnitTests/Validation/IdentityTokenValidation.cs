// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using Xunit;

#if DNXCORE50
using System.IdentityModel.Tokens.Jwt;
#endif

namespace IdentityServer4.Tests.Validation
{

    public class IdentityTokenValidation
    {
        const string Category = "Identity token validation";

        static IdentityTokenValidation()
        {
#if DNXCORE50
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();
#endif

#if DNX451
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
#endif
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_IdentityToken_DefaultKeyType()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var token = TokenFactory.CreateIdentityToken("roclient", "valid");
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator();
            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_IdentityToken_DefaultKeyType_no_ClientId_supplied()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_IdentityToken_no_ClientId_supplied()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt);
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task IdentityToken_InvalidClientId()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "invalid");
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task IdentityToken_Too_Long()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityTokenLong("roclient", "valid", 1000));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }
    }
}