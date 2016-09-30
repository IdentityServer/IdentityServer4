﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.TokenRequest
{
    public class TokenRequestValidation_PKCE
    {
        const string Category = "TokenRequest Validation - PKCE";

        IClientStore _clients = Factory.CreateClientStore();
        InputLengthRestrictions lengths = new InputLengthRestrictions();

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_pkce_token_request_with_plain_method_should_succeed()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient.pkce");
            var grants = Factory.CreateGrantService();
            var verifier = "x".Repeat(lengths.CodeVerifierMinLength);

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                CodeChallenge = verifier.Sha256(),
                CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain,

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, verifier);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_pkce_token_request_with_plain_method_should_succeed_hybrid()
        {
            var client = await _clients.FindEnabledClientByIdAsync("hybridclient.pkce");
            var grants = Factory.CreateGrantService();
            var verifier = "x".Repeat(lengths.CodeVerifierMinLength);

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                CodeChallenge = verifier.Sha256(),
                CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain,

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, verifier);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_pkce_token_request_with_sha256_method_should_succeed()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient.pkce");
            var grants = Factory.CreateGrantService();

            var verifier = "x".Repeat(lengths.CodeVerifierMinLength);
            var challenge = VerifierToSha256CodeChallenge(verifier);
            
            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                CodeChallenge = challenge.Sha256(),
                CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Sha256,

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, verifier);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task token_request_with_missing_code_challenge_and_verifier_should_fail()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient.pkce");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task token_request_with_missing_code_challenge_should_fail()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient.pkce");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, "x".Repeat(lengths.CodeVerifierMinLength));
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task token_request_with_invalid_verifier_plain_method_should_fail()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient.pkce");
            var grants = Factory.CreateGrantService();
            var verifier = "x".Repeat(lengths.CodeVerifierMinLength);

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                CodeChallenge = verifier.Sha256(),
                CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain,

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, verifier + "invalid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task token_request_with_invalid_verifier_sha256_method_should_fail()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient.pkce");
            var grants = Factory.CreateGrantService();

            var verifier = "x".Repeat(lengths.CodeVerifierMinLength);
            var challenge = VerifierToSha256CodeChallenge(verifier);

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                CodeChallenge = challenge.Sha256(),
                CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Sha256,

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, verifier + "invalid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task pkce_token_request_for_non_pkce_client_should_fail()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();
            var verifier = "x".Repeat(lengths.CodeVerifierMinLength);

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                CodeChallenge = verifier.Sha256(),
                CodeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain,

                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.CodeVerifier, verifier);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        private static string VerifierToSha256CodeChallenge(string codeVerifier)
        {
            var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hashedBytes = codeVerifierBytes.Sha256();
            var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

            return transformedCodeVerifier;
        }
    }
}