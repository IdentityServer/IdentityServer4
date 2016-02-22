// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Validation;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation.AuthorizeRequest
{

    public class Authorize_ClientValidation_Token
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Token")]
        public async Task Mixed_Token_Request_Without_OpenId_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "resource profile");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidScope);
        }
    }
}