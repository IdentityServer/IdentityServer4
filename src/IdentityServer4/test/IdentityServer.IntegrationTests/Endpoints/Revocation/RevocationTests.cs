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
        private const string Category = "RevocationTests endpoint";

        private string client_id = "client";
        private string client_secret = "secret";
        private string redirect_uri = "https://client/callback";

        private string scope_name = "api";
        private string scope_secret = "api_secret";

        private IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();

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
                ClientSecrets = { new Secret("secret".Sha256()) },
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

        private class Tokens
        {
            public Tokens(TokenResponse response)
            {
                AccessToken = response.AccessToken;
                RefreshToken = response.RefreshToken;
            }

            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

        private async Task<Tokens> GetTokensAsync()
        {
            await _mockPipeline.LoginAsync("bob");

            var authorizationResponse = await _mockPipeline.RequestAuthorizationEndpointAsync(
                client_id,
                "code",
                "api offline_access",
                "https://client/callback");

            authorizationResponse.IsError.Should().BeFalse();
            authorizationResponse.Code.Should().NotBeNull();

            var tokenResponse = await _mockPipeline.BackChannelClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Code = authorizationResponse.Code,
                RedirectUri = redirect_uri
            });

            tokenResponse.IsError.Should().BeFalse();
            tokenResponse.AccessToken.Should().NotBeNull();
            tokenResponse.RefreshToken.Should().NotBeNull();

            return new Tokens(tokenResponse);
        }

        private async Task<string> GetAccessTokenForImplicitClientAsync(string clientId)
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

        private Task<bool> IsAccessTokenValidAsync(Tokens tokens)
        {
            return IsAccessTokenValidAsync(tokens.AccessToken);
        }

        private async Task<bool> IsAccessTokenValidAsync(string token)
        {
            var response = await _mockPipeline.BackChannelClient.IntrospectTokenAsync(new TokenIntrospectionRequest
            {
                Address = IdentityServerPipeline.IntrospectionEndpoint,
                ClientId = scope_name,
                ClientSecret = scope_secret,

                Token = token,
                TokenTypeHint = IdentityModel.OidcConstants.TokenTypes.AccessToken
            });

            return response.IsError == false && response.IsActive;
        }

        private async Task<bool> UseRefreshTokenAsync(Tokens tokens)
        {
            var response = await _mockPipeline.BackChannelClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = IdentityServerPipeline.TokenEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                RefreshToken = tokens.RefreshToken
            });

            if (response.IsError)
            {
                return false;
            }

            tokens.AccessToken = response.AccessToken;
            return true;
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Get_request_should_return_405()
        {
            var response = await _mockPipeline.BackChannelClient.GetAsync(IdentityServerPipeline.RevocationEndpoint);

            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Post_without_form_urlencoded_should_return_415()
        {
            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, null);

            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Revoke_valid_access_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Token = tokens.AccessToken
            });

            result.IsError.Should().BeFalse();
            (await IsAccessTokenValidAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Revoke_valid_refresh_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await UseRefreshTokenAsync(tokens)).Should().BeTrue();

            var result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Token = tokens.RefreshToken
            });

            result.IsError.Should().BeFalse();

            (await UseRefreshTokenAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Revoke_invalid_access_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Token = tokens.AccessToken
            });

            result.IsError.Should().BeFalse();

            (await IsAccessTokenValidAsync(tokens)).Should().BeFalse();

            result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Token = tokens.AccessToken
            });

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Revoke_invalid_refresh_token_should_return_success()
        {
            var tokens = await GetTokensAsync();
            (await UseRefreshTokenAsync(tokens)).Should().BeTrue();

            var result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Token = tokens.RefreshToken
            });

            result.IsError.Should().BeFalse();

            (await UseRefreshTokenAsync(tokens)).Should().BeFalse();

            result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = client_secret,

                Token = tokens.RefreshToken
            });

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_client_id_should_return_error()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = "not_valid",
                ClientSecret = client_secret,

                Token = tokens.AccessToken
            });

            result.IsError.Should().BeTrue();
            result.Error.Should().Be("invalid_client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_credentials_should_return_error()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var result = await _mockPipeline.BackChannelClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = IdentityServerPipeline.RevocationEndpoint,
                ClientId = client_id,
                ClientSecret = "not_valid",

                Token = tokens.AccessToken
            });

            result.IsError.Should().BeTrue();
            result.Error.Should().Be("invalid_client");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_token_should_return_error()
        {
            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret }
            };

            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = new TokenRevocationResponse(await response.Content.ReadAsStringAsync());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be("invalid_request");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_token_type_hint_should_return_error()
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

            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = new TokenRevocationResponse(await response.Content.ReadAsStringAsync());
            result.IsError.Should().BeTrue();
            result.Error.Should().Be("unsupported_token_type");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_access_token_but_missing_token_type_hint_should_succeed()
        {
            var tokens = await GetTokensAsync();
            (await IsAccessTokenValidAsync(tokens)).Should().BeTrue();

            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "token", tokens.AccessToken }
            };

            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await IsAccessTokenValidAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_refresh_token_but_missing_token_type_hint_should_succeed()
        {
            var tokens = await GetTokensAsync();
            (await UseRefreshTokenAsync(tokens)).Should().BeTrue();

            var data = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "token", tokens.RefreshToken }
            };

            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await UseRefreshTokenAsync(tokens)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Implicit_client_without_secret_revoking_token_should_succeed()
        {
            var token = await GetAccessTokenForImplicitClientAsync("implicit");

            var data = new Dictionary<string, string>
            {
                { "client_id", "implicit" },
                { "token", token }
            };

            (await IsAccessTokenValidAsync(token)).Should().BeTrue();

            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await IsAccessTokenValidAsync(token)).Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Implicit_and_client_creds_client_without_secret_revoking_token_should_fail()
        {
            var token = await GetAccessTokenForImplicitClientAsync("implicit_and_client_creds");

            var data = new Dictionary<string, string>
            {
                { "client_id", "implicit_and_client_creds" },
                { "token", token }
            };

            (await IsAccessTokenValidAsync(token)).Should().BeTrue();

            var response = await _mockPipeline.BackChannelClient.PostAsync(IdentityServerPipeline.RevocationEndpoint, new FormUrlEncodedContent(data));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await IsAccessTokenValidAsync(token)).Should().BeTrue();
        }
    }
}