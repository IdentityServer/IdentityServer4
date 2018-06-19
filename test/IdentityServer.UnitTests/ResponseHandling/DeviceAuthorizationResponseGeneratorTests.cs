using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IdentityServer4.UnitTests.ResponseHandling
{
    public class DeviceAuthorizationResponseGeneratorTests
    {
        private readonly List<IdentityResource> identityResources = new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile()};
        private readonly List<ApiResource> apiResources = new List<ApiResource> {new ApiResource("resource") {Scopes = new List<Scope> {new Scope("api1")}}};
        private readonly MockDeviceCodeStore deviceCodeStore = new MockDeviceCodeStore();
        private readonly MockUserCodeStore userCodeStore = new MockUserCodeStore();
        private readonly IdentityServerOptions options = new IdentityServerOptions();
        private readonly StubClock clock = new StubClock();
        
        private readonly DeviceAuthorizationResponseGenerator generator;
        private readonly DeviceAuthorizationRequestValidationResult testResult;
        private const string TestBaseUrl = "http://localhost:5000/";

        public DeviceAuthorizationResponseGeneratorTests()
        {
            var resourceStore = new InMemoryResourcesStore(identityResources, apiResources);
            var scopeValidator = new ScopeValidator(resourceStore, new NullLogger<ScopeValidator>());
            
            testResult = new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest
            {
                Client = new Client {ClientId = Guid.NewGuid().ToString()},
                IsOpenIdRequest = true,
                ValidatedScopes = scopeValidator
            });

            generator = new DeviceAuthorizationResponseGenerator(options, new DefaultUserCodeService(new[] {new NumericUserCodeService()}),
                deviceCodeStore, userCodeStore, clock, new NullLogger<DeviceAuthorizationResponseGenerator>());
        }

        [Fact]
        public void ProcessAsync_when_valiationresult_null_exect_exception()
        {
            Func<Task> act = () => generator.ProcessAsync(null, TestBaseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ProcessAsync_when_valiationresult_client_null_exect_exception()
        {
            var validationResult = new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest());
            Func <Task> act = () => generator.ProcessAsync(validationResult, TestBaseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ProcessAsync_when_baseurl_null_exect_exception()
        {
            Func<Task> act = () => generator.ProcessAsync(testResult, null);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task ProcessAsync_when_generated_expect_user_code_stored()
        {
            var creationTime = DateTime.UtcNow;
            clock.UtcNowFunc = () => creationTime;

            testResult.ValidatedRequest.RequestedScopes = new List<string> {"openid", "resource"};

            var response = await generator.ProcessAsync(testResult, TestBaseUrl);

            response.UserCode.Should().NotBeNullOrWhiteSpace();

            var userCode = userCodeStore.Codes[response.UserCode];
            userCode.Should().NotBeNull();
            userCode.DeviceCode.Should().Be(response.DeviceCode);
            userCode.ClientId.Should().Be(testResult.ValidatedRequest.Client.ClientId);
            userCode.Lifetime.Should().Be(testResult.ValidatedRequest.Client.DeviceCodeLifetime);
            userCode.CreationTime.Should().Be(creationTime);

            userCode.RequestedScopes.Should().Contain(testResult.ValidatedRequest.RequestedScopes);
        }

        [Fact]
        public async Task ProcessAsync_when_generated_expect_device_code_stored()
        {
            var creationTime = DateTime.UtcNow;
            clock.UtcNowFunc = () => creationTime;

            var response = await generator.ProcessAsync(testResult, TestBaseUrl);

            response.DeviceCode.Should().NotBeNullOrWhiteSpace();
            response.Interval.Should().Be(options.DeviceFlow.Interval);
            
            var deviceCode = deviceCodeStore.Codes[response.DeviceCode];
            deviceCode.Should().NotBeNull();
            deviceCode.ClientId.Should().Be(testResult.ValidatedRequest.Client.ClientId);
            deviceCode.IsOpenId.Should().Be(testResult.ValidatedRequest.IsOpenIdRequest);
            deviceCode.Lifetime.Should().Be(testResult.ValidatedRequest.Client.DeviceCodeLifetime);
            deviceCode.CreationTime.Should().Be(creationTime);
            deviceCode.Subject.Should().BeNull();
            deviceCode.AuthorizedScopes.Should().BeNull();

            response.DeviceCodeLifetime.Should().Be(deviceCode.Lifetime);
        }

        [Fact]
        public async Task ProcessAsync_when_generated_expect_correct_uris()
        {
            const string baseUrl = "http://localhost:5000/";
            options.UserInteraction.DeviceVerificationUrl = "/device";
            options.UserInteraction.DeviceVerificationUserCodeParameter = "userCode";

            var response = await generator.ProcessAsync(testResult, baseUrl);

            response.VerificationUri.Should().Be("http://localhost:5000/device");
            response.VerificationUriComplete.Should().StartWith("http://localhost:5000/device?userCode=");
        }
    }
}