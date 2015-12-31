/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitTests.Common;
using Xunit;

namespace IdentityServer4.Tests.ResponseHandling
{
    public class AuthorizeInteractionResponseGeneratorTests_Login
    {
        IdentityServerOptions _options = new IdentityServerOptions();
        AuthorizeInteractionResponseGenerator _subject;
        MockConsentService _mockConsentService = new MockConsentService();

        public AuthorizeInteractionResponseGeneratorTests_Login()
        {
            _subject = new AuthorizeInteractionResponseGenerator(
                new FakeLogger<AuthorizeInteractionResponseGenerator>(),
                _options,
                _mockConsentService,
                new FakeUserService(),
                new FakeLocalizationService());
        }

        [Fact]
        public async Task Anonymous_User_must_SignIn()
        {
            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = Principal.Anonymous
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_must_not_SignIn()
        {
            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                Subject = IdentityServerPrincipal.Create("123", "dom")
            };

            var result = await _subject.ProcessInteractionAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_current_Idp_must_not_SignIn()
        {
            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client 
                {
                    IdentityProviderRestrictions = new List<string> 
                    {
                        Constants.BuiltInIdentityProvider
                    }
                }
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_restricted_current_Idp_must_SignIn()
        {
            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    IdentityProviderRestrictions = new List<string> 
                    {
                        "some_idp"
                    }
                }
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_requested_Idp_must_not_SignIn()
        {
            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                 AuthenticationContextReferenceClasses = new List<string>{
                    "idp:" + Constants.BuiltInIdentityProvider
                },
                Subject = IdentityServerPrincipal.Create("123", "dom")
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_different_requested_Idp_must_SignIn()
        {
            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                AuthenticationContextReferenceClasses = new List<string>{
                    "idp:some_idp"
                },
                Subject = IdentityServerPrincipal.Create("123", "dom")
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_local_Idp_must_SignIn_when_global_options_does_not_allow_local_logins()
        {
            _options.AuthenticationOptions.EnableLocalLogin = false;

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    ClientId = "foo",
                    EnableLocalLogin = true
                },
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_local_Idp_must_SignIn_when_client_options_does_not_allow_local_logins()
        {
            _options.AuthenticationOptions.EnableLocalLogin = true;

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    ClientId = "foo",
                    EnableLocalLogin = false
                }
            };

            var result = await _subject.ProcessLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }
    }
}
