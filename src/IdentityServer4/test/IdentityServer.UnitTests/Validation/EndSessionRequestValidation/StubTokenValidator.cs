// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer.UnitTests.Validation.EndSessionRequestValidation
{
    public class StubTokenValidator : ITokenValidator
    {
        public TokenValidationResult AccessTokenValidationResult { get; set; } = new TokenValidationResult();
        public TokenValidationResult IdentityTokenValidationResult { get; set; } = new TokenValidationResult();

        public Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AccessTokenValidationResult);
        }

        public Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IdentityTokenValidationResult);
        }

        public Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
