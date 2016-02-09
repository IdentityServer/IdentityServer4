// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Services;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation.TokenRequest
{
    
    public class TokenRequestValidation_ResourceOwner_Invalid
    {
        const string Category = "TokenRequest Validation - ResourceOwner - Invalid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_GrantType_For_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Scopes()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "unknown");
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope_Multiple()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource unknown");
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scope()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource2");
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scope_Multiple()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource resource2");
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_ResourceOwnerCredentials()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_ResourceOwner_UserName()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_ResourceOwner_Password()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_ResourceOwner_Credentials()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "notbob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
            result.ErrorDescription.Should().Be("Username and/or password incorrect");
        }
    }
}