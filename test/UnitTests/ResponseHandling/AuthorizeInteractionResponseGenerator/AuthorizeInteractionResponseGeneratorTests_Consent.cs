// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UnitTests.Common;
using Xunit;

namespace IdentityServer4.Tests.ResponseHandling
{
    public class AuthorizeInteractionResponseGeneratorTests_Consent
    {
        AuthorizeInteractionResponseGenerator _subject;
        IdentityServerOptions _options = new IdentityServerOptions();
        MockConsentService _mockConsent = new MockConsentService();
        FakeProfileService _fakeUserService = new FakeProfileService();
        FakeLocalizationService _fakeLocalizationService = new FakeLocalizationService();

        void RequiresConsent(bool value)
        {
            _mockConsent.RequiresConsentResult = value;
        }

        private void AssertUpdateConsentNotCalled()
        {
            _mockConsent.ConsentClient.Should().BeNull();
            _mockConsent.ConsentSubject.Should().BeNull();
            _mockConsent.ConsentScopes.Should().BeNull();
        }

        private void AssertUpdateConsentCalled(Client client, ClaimsPrincipal user, params string[] scopes)
        {
            _mockConsent.ConsentClient.Should().BeSameAs(client);
            _mockConsent.ConsentSubject.Should().BeSameAs(user);
            _mockConsent.ConsentScopes.Should().BeEquivalentTo(scopes);
        }

        private void AssertErrorReturnsRequestValues(AuthorizeError error, ValidatedAuthorizeRequest request)
        {
            error.ResponseMode.Should().Be(request.ResponseMode);
            error.ErrorUri.Should().Be(request.RedirectUri);
            error.State.Should().Be(request.State);
        }

        private static IEnumerable<Scope> GetScopes()
        {
            return new Scope[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,

                new Scope
                {
                    Name = "read",
                    DisplayName = "Read data",
                    Type = ScopeType.Resource,
                    Emphasize = false,
                },
                new Scope
                {
                    Name = "write",
                    DisplayName = "Write data",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                },
                new Scope
                {
                    Name = "forbidden",
                    Type = ScopeType.Resource,
                    DisplayName = "Forbidden scope",
                    Emphasize = true
                }
             };
        }
        
        public AuthorizeInteractionResponseGeneratorTests_Consent()
        {
            _subject = new AuthorizeInteractionResponseGenerator(
                new FakeLogger<AuthorizeInteractionResponseGenerator>(),
                _options,
                _mockConsent,
                _fakeUserService,
                _fakeLocalizationService);
        }

        [Fact]
        public void ProcessConsentAsync_NullRequest_Throws()
        {
            Func<Task> act = () => _subject.ProcessConsentAsync(null, new ConsentResponse());

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("request");
        }
        
        [Fact]
        public void ProcessConsentAsync_AllowsNullConsent()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            }; 
            var result = _subject.ProcessConsentAsync(request, null).Result;
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsLogin_Throws()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Login
            };

            Func<Task> act = () => _subject.ProcessConsentAsync(request);

            act.ShouldThrow<ArgumentException>()
                .And.Message.Should().Contain("PromptMode");
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsSelectAccount_Throws()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.SelectAccount
            };

            Func<Task> act = () => _subject.ProcessConsentAsync(request);

            act.ShouldThrow<ArgumentException>()
                .And.Message.Should().Contain("PromptMode");
        }


        [Fact]
        public void ProcessConsentAsync_RequiresConsentButPromptModeIsNone_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.None
            };
            var result = _subject.ProcessConsentAsync(request).Result;

            request.WasConsentShown.Should().BeFalse();
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(OidcConstants.AuthorizeErrors.ConsentRequired);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }
        
        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_NoPriorConsent_ReturnsConsentResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            };
            var result = _subject.ProcessConsentAsync(request).Result;
            request.WasConsentShown.Should().BeFalse();
            result.IsConsent.Should().BeTrue();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_NoPriorConsent_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            };
            var result = _subject.ProcessConsentAsync(request).Result;
            request.WasConsentShown.Should().BeFalse();
            result.IsConsent.Should().BeTrue();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_ConsentNotGranted_ReturnsErrorResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                PromptMode = OidcConstants.PromptModes.Consent
            };

            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] {}
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentNotGranted_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
            };
            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] {}
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGrantedButMissingRequiredScopes_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var client = new Client {};
            var scopeValidator = new ScopeValidator(new InMemoryScopeStore(GetScopes()), new FakeLoggerFactory());
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                RequestedScopes = new List<string> { "openid", "read" },
                ValidatedScopes = scopeValidator,
                Client = client
            };
            var valid = scopeValidator.AreScopesValidAsync(request.RequestedScopes).Result;

            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] { "read" }
            };

            var result = _subject.ProcessConsentAsync(request, consent).Result;
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(OidcConstants.AuthorizeErrors.AccessDenied);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public async Task ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                ValidatedScopes = new ScopeValidator(new InMemoryScopeStore(GetScopes()), new FakeLoggerFactory()),
                Client = new Client {
                    AllowRememberConsent = false
                }
            };
            await request.ValidatedScopes.AreScopesValidAsync(new string[] { "read", "write" });
            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] { "read" }
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.ValidatedScopes.GrantedScopes.Count.Should().Be(1);
            "read".Should().Be(request.ValidatedScopes.GrantedScopes.First().Name);
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeFalse();
            AssertUpdateConsentNotCalled();
        }
        
        [Fact]
        public async Task ProcessConsentAsync_PromptModeConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                ValidatedScopes = new ScopeValidator(new InMemoryScopeStore(GetScopes()), new FakeLoggerFactory()),
                Client = new Client {
                    AllowRememberConsent = false
                }
            };
            await request.ValidatedScopes.AreScopesValidAsync(new string[] { "read", "write" });
            var consent = new ConsentResponse
            {
                RememberConsent = false,
                ScopesConsented = new string[] { "read" }
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            request.ValidatedScopes.GrantedScopes.Count.Should().Be(1);
            "read".Should().Be(request.ValidatedScopes.GrantedScopes.First().Name);
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeFalse();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public async Task ProcessConsentAsync_AllowConsentSelected_SavesConsent()
        {
            RequiresConsent(true);
            var client = new Client { AllowRememberConsent = true };
            var user = new ClaimsPrincipal();
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = OidcConstants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = "https://client.com/callback",
                ValidatedScopes = new ScopeValidator(new InMemoryScopeStore(GetScopes()), new FakeLoggerFactory()),
                Client = client,
                Subject = user
            };
            await request.ValidatedScopes.AreScopesValidAsync(new string[] { "read", "write" });
            var consent = new ConsentResponse
            {
                RememberConsent = true,
                ScopesConsented = new string[] { "read" }
            };
            var result = _subject.ProcessConsentAsync(request, consent).Result;
            AssertUpdateConsentCalled(client, user, "read");
        }

    }
}