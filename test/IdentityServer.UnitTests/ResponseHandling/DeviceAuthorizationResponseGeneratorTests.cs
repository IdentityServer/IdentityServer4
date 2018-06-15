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
        private List<IdentityResource> identityResources = new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile()};
        private List<ApiResource> apiResources = new List<ApiResource> {new ApiResource("resource") {Scopes = new List<Scope> {new Scope("api1")}}};
        private MockDeviceCodeStore deviceCodeStore = new MockDeviceCodeStore();
        private MockUserCodeStore userCodeStore = new MockUserCodeStore();
        private IdentityServerOptions options = new IdentityServerOptions();
        
        private readonly DeviceAuthorizationResponseGenerator generator;
        private readonly DeviceAuthorizationRequestValidationResult testResult;
        private readonly string testBaseUrl = "http://localhost:5000/";

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
                deviceCodeStore, userCodeStore, new StubClock(), new NullLogger<DeviceAuthorizationResponseGenerator>());
        }

        [Fact]
        public void ProcessAsync_when_valiationresult_null_exect_exception()
        {
            Func<Task> act = () => generator.ProcessAsync(null, testBaseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ProcessAsync_when_valiationresult_client_null_exect_exception()
        {
            var validationResult = new DeviceAuthorizationRequestValidationResult(new ValidatedDeviceAuthorizationRequest());
            Func <Task> act = () => generator.ProcessAsync(validationResult, testBaseUrl);
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
            var response = await generator.ProcessAsync(testResult, testBaseUrl);

            response.UserCode.Should().NotBeNullOrWhiteSpace();

            var userCode = userCodeStore.Codes[response.UserCode];
            userCode.Should().NotBeNull();
            userCode.DeviceCode.Should().Be(response.DeviceCode);
            userCode.ClientId.Should().Be(testResult.ValidatedRequest.Client.ClientId);
            userCode.Lifetime.Should().Be(testResult.ValidatedRequest.Client.DeviceCodeLifetime);

            // TODO: verify scopes
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