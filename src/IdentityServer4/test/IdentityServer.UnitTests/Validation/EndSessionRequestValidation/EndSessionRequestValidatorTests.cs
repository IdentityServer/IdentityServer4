﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.EndSessionRequestValidation
{
    public class EndSessionRequestValidatorTests
    {
        private EndSessionRequestValidator _subject;
        private IdentityServerOptions _options;
        private StubTokenValidator _stubTokenValidator = new StubTokenValidator();
        private StubRedirectUriValidator _stubRedirectUriValidator = new StubRedirectUriValidator();
        private MockHttpContextAccessor _context = new MockHttpContextAccessor();
        private MockUserSession _userSession = new MockUserSession();
        private MockMessageStore<EndSession> _mockEndSessionMessageStore = new MockMessageStore<EndSession>();
        private InMemoryClientStore _clientStore;

        private ClaimsPrincipal _user;

        public EndSessionRequestValidatorTests()
        {
            _user = new IdentityServerUser("alice").CreatePrincipal();
            _clientStore = new InMemoryClientStore(new Client[0]);

            _options = TestIdentityServerOptions.Create();
            _subject = new EndSessionRequestValidator(
                _context,
                _options,
                _stubTokenValidator,
                _stubRedirectUriValidator,
                _userSession,
                _clientStore,
                _mockEndSessionMessageStore,
                TestLogger.Create<EndSessionRequestValidator>());
        }

        [Fact]
        public async Task anonymous_user_when_options_require_authenticated_user_should_return_error()
        {
            _options.Authentication.RequireAuthenticatedUserForSignOutMessage = true;

            var parameters = new NameValueCollection();
            var result = await _subject.ValidateAsync(parameters, null);
            result.IsError.Should().BeTrue();

            result = await _subject.ValidateAsync(parameters, new ClaimsPrincipal());
            result.IsError.Should().BeTrue();

            result = await _subject.ValidateAsync(parameters, new ClaimsPrincipal(new ClaimsIdentity()));
            result.IsError.Should().BeTrue();
        }

        [Fact]
        public async Task valid_params_should_return_success()
        {
            _stubTokenValidator.IdentityTokenValidationResult = new TokenValidationResult()
            {
                IsError = false,
                Claims = new Claim[] { new Claim("sub", _user.GetSubjectId()) },
                Client = new Client() { ClientId = "client"}
            };
            _stubRedirectUriValidator.IsPostLogoutRedirectUriValid = true;

            var parameters = new NameValueCollection();
            parameters.Add("id_token_hint", "id_token");
            parameters.Add("post_logout_redirect_uri", "http://client/signout-cb");
            parameters.Add("client_id", "client1");
            parameters.Add("state", "foo");

            var result = await _subject.ValidateAsync(parameters, _user);
            result.IsError.Should().BeFalse();

            result.ValidatedRequest.Client.ClientId.Should().Be("client");
            result.ValidatedRequest.PostLogOutUri.Should().Be("http://client/signout-cb");
            result.ValidatedRequest.State.Should().Be("foo");
            result.ValidatedRequest.Subject.GetSubjectId().Should().Be(_user.GetSubjectId());
        }

        [Fact]
        public async Task no_post_logout_redirect_uri_should_use_single_registered_uri()
        {
            _stubTokenValidator.IdentityTokenValidationResult = new TokenValidationResult()
            {
                IsError = false,
                Claims = new Claim[] { new Claim("sub", _user.GetSubjectId()) },
                Client = new Client() { ClientId = "client1", PostLogoutRedirectUris = new List<string> { "foo" } }
            };
            _stubRedirectUriValidator.IsPostLogoutRedirectUriValid = true;

            var parameters = new NameValueCollection();
            parameters.Add("id_token_hint", "id_token");

            var result = await _subject.ValidateAsync(parameters, _user);
            result.IsError.Should().BeFalse();
            result.ValidatedRequest.PostLogOutUri.Should().Be("foo");
        }

        [Fact]
        public async Task no_post_logout_redirect_uri_should_not_use_multiple_registered_uri()
        {
            _stubTokenValidator.IdentityTokenValidationResult = new TokenValidationResult()
            {
                IsError = false,
                Claims = new Claim[] { new Claim("sub", _user.GetSubjectId()) },
                Client = new Client() { ClientId = "client1", PostLogoutRedirectUris = new List<string> { "foo", "bar" } }
            };
            _stubRedirectUriValidator.IsPostLogoutRedirectUriValid = true;

            var parameters = new NameValueCollection();
            parameters.Add("id_token_hint", "id_token");

            var result = await _subject.ValidateAsync(parameters, _user);
            result.IsError.Should().BeFalse();
            result.ValidatedRequest.PostLogOutUri.Should().BeNull();
        }

        [Fact]
        public async Task post_logout_uri_fails_validation_should_return_error()
        {
            _stubTokenValidator.IdentityTokenValidationResult = new TokenValidationResult()
            {
                IsError = false,
                Claims = new Claim[] { new Claim("sub", _user.GetSubjectId()) },
                Client = new Client() { ClientId = "client" }
            };
            _stubRedirectUriValidator.IsPostLogoutRedirectUriValid = false;

            var parameters = new NameValueCollection();
            parameters.Add("id_token_hint", "id_token");
            parameters.Add("post_logout_redirect_uri", "http://client/signout-cb");
            parameters.Add("client_id", "client1");
            parameters.Add("state", "foo");

            var result = await _subject.ValidateAsync(parameters, _user);
            result.IsError.Should().BeTrue();
        }

        [Fact]
        public async Task subject_mismatch_should_return_error()
        {
            _stubTokenValidator.IdentityTokenValidationResult = new TokenValidationResult()
            {
                IsError = false,
                Claims = new Claim[] { new Claim("sub", "xoxo") },
                Client = new Client() { ClientId = "client" }
            };
            _stubRedirectUriValidator.IsPostLogoutRedirectUriValid = true;

            var parameters = new NameValueCollection();
            parameters.Add("id_token_hint", "id_token");
            parameters.Add("post_logout_redirect_uri", "http://client/signout-cb");
            parameters.Add("client_id", "client1");
            parameters.Add("state", "foo");

            var result = await _subject.ValidateAsync(parameters, _user);
            result.IsError.Should().BeTrue();
        }

        [Fact]
        public async Task successful_request_should_return_inputs()
        {
            var parameters = new NameValueCollection();

            var result = await _subject.ValidateAsync(parameters, _user);
            result.IsError.Should().BeFalse();
            result.ValidatedRequest.Raw.Should().BeSameAs(parameters);
        }
    }
}
