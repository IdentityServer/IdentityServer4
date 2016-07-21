// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Services;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation.TokenRequest
{

    public class TokenRequestValidation_ExtensionGrants_Invalid
    {
        const string Category = "TokenRequest Validation - AssertionFlow - Invalid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Extension_Grant_Type_For_Client_Credentials_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "customGrant");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Extension_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "unknown_grant_type");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }
    }
}