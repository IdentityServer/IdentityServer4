// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Services.Default;
using IdentityServer4.UnitTests.Common;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultClaimsServiceTests
    {
        DefaultClaimsService _subject;
        MockProfileService _mockMockProfileService = new MockProfileService();

        public DefaultClaimsServiceTests()
        {
            _subject = new DefaultClaimsService(_mockMockProfileService, TestLogger.Create<DefaultClaimsService>());
        }

        [Fact(Skip = "incomplete")]
        public async Task tests_should_have_a_name()
        {
            var user = IdentityServerPrincipal.Create("bob", "bob", new Claim[] {
                new Claim("foo", "foo1"),
                new Claim("foo", "foo2"),
                new Claim("bar", "bar1"),
                new Claim("bar", "bar2"),
            });
        }

    }
}
