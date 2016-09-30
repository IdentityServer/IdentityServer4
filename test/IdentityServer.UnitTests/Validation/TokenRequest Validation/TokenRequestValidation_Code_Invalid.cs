﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.TokenRequest
{
    public class TokenRequestValidation_Code_Invalid
    {
        IClientStore _clients = Factory.CreateClientStore();
        const string Category = "TokenRequest Validation - AuthorizationCode - Invalid";

        ClaimsPrincipal _subject = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_AuthorizationCode()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_AuthorizationCode()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "invalid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task AuthorizationCodeTooLong()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);
            var longCode = "x".Repeat(options.InputLengthRestrictions.AuthorizationCode + 1);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, longCode);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Scopes_for_AuthorizationCode()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
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
            OidcConstants.TokenErrors.InvalidRequest.Should().Be(result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_Not_Authorized_For_AuthorizationCode_Flow()
        {
            var client = await _clients.FindEnabledClientByIdAsync("implicitclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
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
            result.Error.Should().Be(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_Trying_To_Request_Token_Using_Another_Clients_Code()
        {
            var client1 = await _clients.FindEnabledClientByIdAsync("codeclient");
            var client2 = await _clients.FindEnabledClientByIdAsync("codeclient_restricted");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client1.ClientId,
                Lifetime = client1.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client2);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_RedirectUri()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Different_RedirectUri_Between_Authorize_And_Token_Request()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server1/cb",
                Subject = _subject
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server2/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_AuthorizationCode()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                CreationTime = DateTime.UtcNow.AddSeconds(-100),
                Subject = _subject
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
        public async Task Reused_AuthorizationCode()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
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

            // request first time
            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();

            // request second time
            validator = Factory.CreateTokenRequestValidator(
                grants: grants);
            
            result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Code_Request_with_disabled_User()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateGrantService();

            var code = new AuthorizationCode
            {
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            await grants.StoreAuthorizationCodeAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                grants: grants,
                profile: new TestProfileService(shouldBeActive: false));

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }
    }
}