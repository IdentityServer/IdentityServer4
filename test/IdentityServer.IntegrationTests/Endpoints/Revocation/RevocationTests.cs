// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel.Client;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.IntegrationTests.Endpoints.Revocation
{
    public class RevocationTests
    {
        const string Category = "RevocationTests endpoint";

        string client_id = "client";
        string client_secret = "secret";
        string redirect_uri = "https://client/callback";

        string scope_name = "api";
        string scope_secret = "api_secret";

        MockIdSvrUiPipeline _mockPipeline = new MockIdSvrUiPipeline();

        public RevocationTests()
        {
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = client_id,
                ClientSecrets = new List<Secret> { new Secret(client_secret.Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                AllowOfflineAccess = true,
                AllowedScopes = new List<string> { "api" },
                RedirectUris = new List<string> { redirect_uri },
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference,
                RefreshTokenUsage = TokenUsage.ReUse
            });
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "implicit",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                AllowedScopes = new List<string> { "api" },
                RedirectUris = new List<string> { redirect_uri },
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference
            });
            _mockPipeline.Clients.Add(new Client
            {
                ClientId = "implicit_and_client_creds",
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                RequireConsent = false,
                AllowedScopes = new List<string> { "api" },
                RedirectUris = new List<string> { redirect_uri },
                AllowAccessTokensViaBrowser = true,
                AccessTokenType = AccessTokenType.Reference
            });

            _mockPipeline.Users.Add(new TestUser
            {
                SubjectId = "bob",
                Username = "bob",
                Claims = new Claim[]
                {
                    new Claim("name", "Bob Loblaw"),
                    new Claim("email", "bob@loblaw.com"),
                    new Claim("role", "Attorney")
                }
            });

            _mockPipeline.IdentityScopes.AddRange(new IdentityResource[] {
                new IdentityResources.OpenId()
            });

            _mockPipeline.ApiScopes.AddRange(new ApiResource[] {
                new ApiResource
                {
                    Name = "api",
                    ApiSecrets = new List<Secret> { new Secret(scope_secret.Sha256()) },
                    Scopes =
                    {
                        new Scope
                        {
                            Name = scope_name
                        }
                    }
                }
            });

            _mockPipeline.Initialize();
        }

        class Tokens
        {
            public Tokens(IdentityModel.Client.TokenResponse response)
            {
                AccessToken = response.AccessToken;
                RefreshToken = response.RefreshToken;
            }

            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

        async Task<Tokens> GetTokensAsync()
        {
            await _mockPipeline.LoginAsync("bob");

            var authorizationResponse = await _mockPipeline.RequestAuthorizationEndpointAsync(
                client_id,
                "code",
                "api offline_access",
                "https://client/callback");

            authorizationResponse.IsError.Should().BeFalse();
            authorizationResponse.Code.Should().NotBeNull();

            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _mockPipeline.Handler);
            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(authorizationResponse.Code, redirect_uri);

            tokenResponse.IsError.Should().BeFalse();
            tokenResponse.AccessToken.Should().NotBeNull();
            tokenResponse.RefreshToken.Should().NotBeNull();

            return new Tokens(tokenResponse);
        }

        async Task<string> GetAccessTokenForImplicitClientAsync(string clientId)
        {
            await _mockPipeline.LoginAsync("bob");

            var authorizationResponse = await _mockPipeline.RequestAuthorizationEndpointAsync(
                clientId,
                "token",
                "api",
                "https://client/callback");

            authorizationResponse.IsError.Should().BeFalse();
            authorizationResponse.AccessToken.Should().NotBeNull();

            return authorizationResponse.AccessToken;
        }

        Task<bool> IsAccessTokenValidAsync(Tokens tokens)
        {
            return IsAccessTokenValidAsync(tokens.AccessToken);
        }

        async Task<bool> IsAccessTokenValidAsync(string token)
        {
            var introspectionClient = new IntrospectionClient(MockIdSvrUiPipeline.IntrospectionEndpoint, scope_name, scope_secret, _mockPipeline.Handler);
            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = token,
                TokenTypeHint = IdentityModel.OidcConstants.TokenTypes.AccessToken
            });
            return response.IsError == false && response.IsActive;
        }

        async Task<bool> UseRefreshTokenAsync(Tokens tokens)
        {
            var tokenClient = new TokenClient(MockIdSvrUiPipeline.TokenEndpoint, client_id, client_secret, _mockPipeline.Handler);
            var response = await tokenClient.RequestRefreshTokenAsync(tokens.RefreshToken);
            if (response.IsError)
            {
                return false;
            }
            tokens.AccessToken = response.AccessToken;
            return true;
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_return_405()
        {
            var response = await _mockPipeline.Client.GetAsync(MockIdSvrUiPipeline.RevocationEndpoint);

            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task post_without_form_urlencoded_should_return_415()
        {
            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, null);

            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task revoke_valid_access_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var revocationClient = new TokenRevocationClient(MockIdSvrUiPipeline.RevocationEndpoint, client_id, client_secret, _mockPipeline.Handler);
            var result = await revocationClient.RevokeAccessTokenAsync(tokens.AccessToken);
            result.IsError.Should().BeFalse();

            (await IsAccessTokenValidAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task revoke_valid_refresh_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await UseRefreshTokenAsync(tokens)).Should().BeTrue();

            var revocationClient = new TokenRevocationClient(MockIdSvrUiPipeline.RevocationEndpoint, client_id, client_secret, _mockPipeline.Handler);
            var result = await revocationClient.RevokeRefreshTokenAsync(tokens.RefreshToken);
            result.IsError.Should().BeFalse();

            (await UseRefreshTokenAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task revoke_invalid_access_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var revocationClient = new TokenRevocationClient(MockIdSvrUiPipeline.RevocationEndpoint, client_id, client_secret, _mockPipeline.Handler);
            var result = await revocationClient.RevokeAccessTokenAsync(tokens.AccessToken);
            result.IsError.Should().BeFalse();

            (await IsAccessTokenValidAsync(tokens)).Should().BeFalse();

            result = await revocationClient.RevokeAccessTokenAsync(tokens.AccessToken);
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task revoke_invalid_refresh_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await UseRefreshTokenAsync(tokens)).Should().BeTrue();

            var revocationClient = new TokenRevocationClient(MockIdSvrUiPipeline.RevocationEndpoint, client_id, client_secret, _mockPipeline.Handler);
            var result = await revocationClient.RevokeRefreshTokenAsync(tokens.RefreshToken);
            result.IsError.Should().BeFalse();

            (await UseRefreshTokenAsync(tokens)).Should().BeFalse();

            result = await revocationClient.RevokeRefreshTokenAsync(tokens.RefreshToken);
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task invalid_client_id_should_return_error()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var revocationClient = new TokenRevocationClient(MockIdSvrUiPipeline.RevocationEndpoint, "not_valid", client_secret, _mockPipeline.Handler);
            var result = await revocationClient.RevokeAccessTokenAsync(tokens.AccessToken);
            result.IsError.Should().BeTrue();
            result.Error.Should().Be("invalid_client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task invalid_credentials_should_return_error()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var revocationClient = new TokenRevocationClient(MockIdSvrUiPipeline.RevocationEndpoint, client_id, "not_valid", _mockPipeline.Handler);
            var result = await revocationClient.RevokeAccessTokenAsync(tokens.AccessToken);
            result.IsError.Should().BeTrue();
            result.Error.Should().Be("invalid_client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task missing_token_should_return_error()
        {
            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret }
            };

            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = new TokenRevocationResponse(await response.Content.ReadAsStringAsync());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be("invalid_request");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task invalid_token_type_hint_should_return_error()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "token", tokens.AccessToken },
                { "token_type_hint", "not_valid" }
            };

            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = new TokenRevocationResponse(await response.Content.ReadAsStringAsync());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be("unsupported_token_type");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_access_token_but_missing_token_type_hint_should_succeed()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "token", tokens.AccessToken }
            };

            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await IsAccessTokenValidAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task valid_refresh_token_but_missing_token_type_hint_should_succeed()
        {
            var tokens = await GetTokensAsync();
            (await UseRefreshTokenAsync(tokens)).Should().BeTrue();

            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "token", tokens.RefreshToken }
            };

            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await UseRefreshTokenAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task implicit_client_without_secret_revoking_token_should_succeed()
        {
            var token = await GetAccessTokenForImplicitClientAsync("implicit");

            var data = new Dictionary<string, string>
            {
                { "client_id", "implicit" },
                { "token", token }
            };

            (await IsAccessTokenValidAsync(token)).Should().BeTrue();

            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await IsAccessTokenValidAsync(token)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task implicit_and_client_creds_client_without_secret_revoking_token_should_fail()
        {
            var token = await GetAccessTokenForImplicitClientAsync("implicit_and_client_creds");

            var data = new Dictionary<string, string>
            {
                { "client_id", "implicit_and_client_creds" },
                { "token", token }
            };

            (await IsAccessTokenValidAsync(token)).Should().BeTrue();

            var response = await _mockPipeline.Client.PostAsync(MockIdSvrUiPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await IsAccessTokenValidAsync(token)).Should().BeTrue();
        }
    }
}
