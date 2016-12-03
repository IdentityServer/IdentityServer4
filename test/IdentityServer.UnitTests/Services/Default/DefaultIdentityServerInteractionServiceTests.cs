// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services.Default;
using IdentityServer4.UnitTests.Common;
using System.Security.Claims;
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
            _mockMockHttpContextAccessor = new MockHttpContextAccessor(_options);

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
    }
}
