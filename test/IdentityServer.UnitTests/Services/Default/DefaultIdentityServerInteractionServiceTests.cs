// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.UnitTests.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultIdentityServerInteractionServiceTests
    {
        DefaultIdentityServerInteractionService _subject;

        IdentityServerOptions _options = new IdentityServerOptions();
        MockHttpContextAccessor _mockMockHttpContextAccessor;
        MockMessageStore<LogoutMessage> _mockLogoutMessageStore = new MockMessageStore<LogoutMessage>();
        MockMessageStore<Models.ErrorMessage> _mockErrorMessageStore = new MockMessageStore<Models.ErrorMessage>();
        MockMessageStore<ConsentResponse> _mockConsentStore = new MockMessageStore<ConsentResponse>();
        MockPersistedGrantService _mockPersistedGrantService = new MockPersistedGrantService();
        MockClientSessionService _mockClientSessionService = new MockClientSessionService();
        MockSessionIdService _mockSessionIdService = new MockSessionIdService();
        MockReturnUrlParser _mockReturnUrlParser = new MockReturnUrlParser();

        public DefaultIdentityServerInteractionServiceTests()
        {
            _mockMockHttpContextAccessor = new MockHttpContextAccessor(_options, _mockSessionIdService, _mockClientSessionService);

            _subject = new DefaultIdentityServerInteractionService(_options, 
                _mockMockHttpContextAccessor,
                _mockLogoutMessageStore,
                _mockErrorMessageStore,
                _mockConsentStore,
                _mockPersistedGrantService,
                _mockClientSessionService,
                _mockSessionIdService,
                _mockReturnUrlParser,
                TestLogger.Create<DefaultIdentityServerInteractionService>()
            );
        }
        
        [Fact]
        public async Task GetLogoutContextAsync_valid_session_and_logout_id_should_provide_iframe_with_logoutid_param()
        {
            // for this, we're just ensuring that the session is used from the message, not from the current session cookie
            _mockSessionIdService.SessionId = null;
            _mockLogoutMessageStore.Messages.Add("id", new Message<LogoutMessage>(new LogoutMessage() { SessionId = "session" }));

            var context = await _subject.GetLogoutContextAsync("id");

            context.SignOutIFrameUrl.Should().NotBeNull();
            context.SignOutIFrameUrl.Should().Contain(_options.UserInteraction.LogoutIdParameter + "=id");
        }

        [Fact]
        public async Task GetLogoutContextAsync_valid_session_no_logout_id_should_provide_iframe()
        {
            _mockSessionIdService.SessionId = "session";
            var context = await _subject.GetLogoutContextAsync(null);

            context.SignOutIFrameUrl.Should().NotBeNull();
            context.SignOutIFrameUrl.Should().NotContain(_options.UserInteraction.LogoutIdParameter + "=");
        }

        [Fact]
        public async Task GetLogoutContextAsync_without_session_should_not_provide_iframe()
        {
            _mockSessionIdService.SessionId = null;
            _mockLogoutMessageStore.Messages.Add("id", new Message<LogoutMessage>(new LogoutMessage()));

            var context = await _subject.GetLogoutContextAsync("id");

            context.SignOutIFrameUrl.Should().BeNull();
        }

        [Fact]
        public async Task CreateLogoutContextAsync_without_session_should_not_create_session()
        {
            _mockSessionIdService.SessionId = null;

            var context = await _subject.CreateLogoutContextAsync();

            context.Should().BeNull();
            _mockLogoutMessageStore.Messages.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateLogoutContextAsync_with_session_should_create_session()
        {
            _mockSessionIdService.SessionId = "session";

            var context = await _subject.CreateLogoutContextAsync();

            context.Should().NotBeNull();
            _mockLogoutMessageStore.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public void GrantConsentAsync_should_throw_if_no_subject()
        {
            Func<Task> act = () => _subject.GrantConsentAsync(new AuthorizationRequest(), new ConsentResponse(), null);

            act.ShouldThrow<ArgumentNullException>()
                .And.Message.Should().Contain("subject");
        }

        [Fact]
        public async Task GrantConsentAsync_should_use_current_subject_and_create_message()
        {
            var user = IdentityServerPrincipal.Create("bob", "bob");
            _mockMockHttpContextAccessor.HttpContext.SetUser(user);

            var req = new AuthorizationRequest() { ClientId = "client" };
            await _subject.GrantConsentAsync(req, new ConsentResponse(), null);

            _mockConsentStore.Messages.Should().NotBeEmpty();
            var consentRequest = new ConsentRequest(req, "bob");
            _mockConsentStore.Messages.First().Key.Should().Be(consentRequest.Id);
        }
    }
}
