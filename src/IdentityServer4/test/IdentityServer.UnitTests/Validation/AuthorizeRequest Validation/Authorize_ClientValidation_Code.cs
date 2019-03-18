﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.UnitTests.Common;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.AuthorizeRequest
{
    public class Authorize_ClientValidation_Code
    {
        private IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "unknown");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidScope);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://invalid");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdToken);
            parameters.Add(OidcConstants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken);
            parameters.Add(OidcConstants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Unknown_Client()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "unknown");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Restricted_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient_restricted");
            parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
        }
    }
}