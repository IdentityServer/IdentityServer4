// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation.TokenRequest
{
    public class TokenRequestValidation_Valid
    {
        const string Category = "TokenRequest Validation - General - Valid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Code_Request()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                Client = client,
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Code_Request_with_Refresh_Token()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    },
                    new Scope
                    {
                        Name = "offline_access"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request()
        {
            var client = await _clients.FindClientByIdAsync("client");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request_for_Implicit_and_ClientCredentials_Client()
        {
            var client = await _clients.FindClientByIdAsync("implicit_and_client_creds_client");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request_Restricted_Client()
        {
            var client = await _clients.FindClientByIdAsync("client_restricted");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ResourceOwner_Request()
        {
            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ResourceOwner_Request_with_Refresh_Token()
        {
            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource offline_access");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ResourceOwner_Request_Restricted_Client()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_CustomGrant_Request()
        {
            var client = await _clients.FindClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "custom_grant");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
        }

        // todo
        //[Fact]
        //[Trait("Category", Category)]
        //public async Task Valid_RefreshToken_Request()
        //{
        //    var mock = new Mock<IUserService>();
        //    var subjectClaim = new Claim(Constants.ClaimTypes.Subject, "foo");

        //    var refreshToken = new RefreshToken
        //    {
        //        AccessToken = new Token("access_token")
        //        {
        //            Claims = new List<Claim> { subjectClaim },
        //            Client =new Client{ClientId = "roclient"}
        //        },
        //        LifeTime = 600,
        //        CreationTime = DateTimeOffset.UtcNow
        //    };
        //    var handle = Guid.NewGuid().ToString();

        //    var store = new InMemoryRefreshTokenStore();
        //    await store.StoreAsync(handle, refreshToken);

        //    var client = await _clients.FindClientByIdAsync("roclient");

        //    var validator = Factory.CreateTokenRequestValidator(
        //        refreshTokens: store,
        //        userService: mock.Object);

        //    var parameters = new NameValueCollection();
        //    parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
        //    parameters.Add(Constants.TokenRequest.RefreshToken, handle);

        //    var result = await validator.ValidateRequestAsync(parameters, client);

        //    result.IsError.Should().BeFalse();
        //}

        // todo
        //[Fact]
        //[Trait("Category", Category)]
        //public async Task Valid_RefreshToken_Request_using_Restricted_Client()
        //{
        //    var mock = new Mock<IUserService>();
        //    var subjectClaim = new Claim(Constants.ClaimTypes.Subject, "foo");

        //    var refreshToken = new RefreshToken
        //    {
        //        AccessToken = new Token("access_token")
        //        {
        //            Claims = new List<Claim> { subjectClaim },
        //            Client = new Client { ClientId = "roclient_restricted_refresh"}
        //        },
                
        //        LifeTime = 600,
        //        CreationTime = DateTimeOffset.UtcNow
        //    };
        //    var handle = Guid.NewGuid().ToString();

        //    var store = new InMemoryRefreshTokenStore();
        //    await store.StoreAsync(handle, refreshToken);

        //    var client = await _clients.FindClientByIdAsync("roclient_restricted_refresh");

        //    var validator = Factory.CreateTokenRequestValidator(
        //        refreshTokens: store,
        //        userService: mock.Object);

        //    var parameters = new NameValueCollection();
        //    parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
        //    parameters.Add(Constants.TokenRequest.RefreshToken, handle);

        //    var result = await validator.ValidateRequestAsync(parameters, client);

        //    result.IsError.Should().BeFalse();
        //}
    }
}