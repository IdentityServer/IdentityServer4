﻿using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static IdentityModel.OidcConstants;

namespace IdentityServer.IntegrationTests.Conformance.Pkce
{
    public class PkceTests
    {
        const string Category = "PKCE";

        MockIdSvrUiPipeline _pipeline = new MockIdSvrUiPipeline();

        Client client;
        string client_id = "codewithproofkey_client";
        string redirect_uri = "https://code_client/callback";
        string code_verifier = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        string client_secret = "secret";
        string response_type = "code";

        public PkceTests()
        {
            _pipeline.Users.Add(new InMemoryUser
            {
                Subject = "bob",
                Username = "bob",
                Claims = new Claim[]
                {
                        new Claim("name", "Bob Loblaw"),
                        new Claim("email", "bob@loblaw.com"),
                        new Claim("role", "Attorney"),
                }
            });
            _pipeline.Scopes.Add(StandardScopes.OpenId);
            _pipeline.Clients.Add(client = new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                AllowedGrantTypes = IdentityServer4.Models.GrantTypes.Code,
                RequirePkce = true,

                AllowAccessToAllScopes = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });

            _pipeline.Initialize();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_can_use_plain_code_challenge_method()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: CodeChallengeMethods.Plain);

            authorizeResponse.IsError.Should().BeFalse();

            var code = authorizeResponse.Code;

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _pipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(code, redirect_uri, code_verifier);

            tokenResponse.IsError.Should().BeFalse();
            tokenResponse.TokenType.Should().Be("Bearer");
            tokenResponse.AccessToken.Should().NotBeNull();
            tokenResponse.IdentityToken.Should().NotBeNull();
            tokenResponse.ExpiresIn.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_can_use_sha256_code_challenge_method()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = Sha256OfCodeVerifier(code_verifier);
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: CodeChallengeMethods.Sha256);

            authorizeResponse.IsError.Should().BeFalse();

            var code = authorizeResponse.Code;

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _pipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(code, redirect_uri, code_verifier);

            tokenResponse.IsError.Should().BeFalse();
            tokenResponse.TokenType.Should().Be("Bearer");
            tokenResponse.AccessToken.Should().NotBeNull();
            tokenResponse.IdentityToken.Should().NotBeNull();
            tokenResponse.ExpiresIn.Should().BeGreaterThan(0);
        }

        [Fact(Skip = "need url decoding on error_message")]
        [Trait("Category", Category)]
        public async Task Authorize_request_needs_code_challenge()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce);

            authorizeResponse.IsError.Should().BeTrue();
            authorizeResponse.Error.Should().Be(AuthorizeErrors.InvalidRequest);
            authorizeResponse.Values["error_description"].Should().Be("code%20challenge%20required");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Authorize_request_code_challenge_cannot_be_too_short()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge:"a");

            authorizeResponse.IsError.Should().BeTrue();
            authorizeResponse.Error.Should().Be(AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Authorize_request_code_challenge_cannot_be_too_long()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: new string('a', _pipeline.Options.InputLengthRestrictions.CodeChallengeMaxLength + 1)
            );

            authorizeResponse.IsError.Should().BeTrue();
            authorizeResponse.Error.Should().Be(AuthorizeErrors.InvalidRequest);
        }

        [Fact(Skip = "need url decoding on error_message")]
        [Trait("Category", Category)]
        public async Task Authorize_request_needs_supported_code_challenge_method()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: "unknown_code_challenge_method"
            );

            authorizeResponse.IsError.Should().BeTrue();
            authorizeResponse.Error.Should().Be(AuthorizeErrors.InvalidRequest);
            authorizeResponse.Values["error_description"].Should().Be("transform%20algorithm%20not%20supported");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_request_needs_code_verifier()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: CodeChallengeMethods.Plain);

            authorizeResponse.IsError.Should().BeFalse();

            var code = authorizeResponse.Code;

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _pipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(code, redirect_uri);

            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be(TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_request_code_verifier_cannot_be_too_short()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: CodeChallengeMethods.Plain);

            authorizeResponse.IsError.Should().BeFalse();

            var code = authorizeResponse.Code;

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _pipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(code, redirect_uri,
                "a");

            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be(TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_request_code_verifier_cannot_be_too_long()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: CodeChallengeMethods.Plain);

            authorizeResponse.IsError.Should().BeFalse();

            var code = authorizeResponse.Code;

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _pipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(code, redirect_uri,
                new string('a', _pipeline.Options.InputLengthRestrictions.CodeVerifierMaxLength + 1));

            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be(TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Token_request_code_verifier_must_match_with_code_chalenge()
        {
            await _pipeline.LoginAsync("bob");

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeResponse = await _pipeline.RequestAuthorizationEndpointAsync(client_id,
                response_type,
                Constants.StandardScopes.OpenId,
                redirect_uri,
                nonce: nonce,
                codeChallenge: code_challenge,
                codeChallengeMethod: CodeChallengeMethods.Plain);

            authorizeResponse.IsError.Should().BeFalse();

            var code = authorizeResponse.Code;

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _pipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(code, redirect_uri,
                "mismatched_code_verifier");

            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be(TokenErrors.InvalidGrant);
        }

        private static string Sha256OfCodeVerifier(string codeVerifier)
        {
            var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hashedBytes = codeVerifierBytes.Sha256();
            var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

            return transformedCodeVerifier;
        }
    }
}
