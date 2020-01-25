// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Common;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class DefaultTokenServiceTests
    {
        private DefaultTokenService _subject;

        MockClaimsService _mockClaimsService = new MockClaimsService();
        MockReferenceTokenStore _mockReferenceTokenStore = new MockReferenceTokenStore();
        MockTokenCreationService _mockTokenCreationService = new MockTokenCreationService();
        DefaultHttpContext _httpContext = new DefaultHttpContext();
        MockSystemClock _mockSystemClock = new MockSystemClock();
        MockKeyMaterialService _mockKeyMaterialService = new MockKeyMaterialService();
        IdentityServerOptions _options = new IdentityServerOptions();

        public DefaultTokenServiceTests()
        {
            _options.IssuerUri = "https://test.identityserver.io";

            var svcs = new ServiceCollection();
            svcs.AddSingleton(_options);
            _httpContext.RequestServices = svcs.BuildServiceProvider();

            _subject = new DefaultTokenService(
                _mockClaimsService,
                _mockReferenceTokenStore,
                _mockTokenCreationService,
                new HttpContextAccessor { HttpContext = _httpContext },
                _mockSystemClock,
                _mockKeyMaterialService,
                _options,
                TestLogger.Create<DefaultTokenService>());
        }

        [Fact]
        public async Task CreateAccessTokenAsync_should_include_aud_for_each_ApiResource()
        {
            var scope = new Scope() { Name = "resource" };

            var request = new TokenCreationRequest { 
                ValidatedResources = new ResourceValidationResult()
                {
                    ApiResources = 
                    {
                        new ApiResource("api1"){ Scopes = { scope.Name } },
                        new ApiResource("api2"){ Scopes = { scope.Name } },
                        new ApiResource("api3"){ Scopes = { scope.Name } },
                    },
                    Scopes = 
                    {
                        new ScopeValue("res1", scope),
                        new ScopeValue("res2", scope),
                        new ScopeValue("res3", scope),
                    }
                },
                ValidatedRequest = new ValidatedRequest()
                {
                    Client = new Client { }
                }
            };

            var result = await _subject.CreateAccessTokenAsync(request);

            result.Audiences.Count.Should().Be(3);
            result.Audiences.Should().BeEquivalentTo(new[] { "api1", "api2", "api3" });
        }
    }
}
