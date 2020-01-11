using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class StrictRedirectUriValidatorAppAuthValidation
    {
        private const string Category = "Strict Redirect Uri Validator AppAuth Validation";

        private Client clientWithValidLoopbackRedirectUri = new Client 
        {
            RequirePkce = true,
            RedirectUris = new List<string>
            {
                "http://127.0.0.1"
            }
        };

        private Client clientWithNoRedirectUris = new Client
        {
            RequirePkce = true
        };

        [Theory]
        [Trait("Category", Category)]
        [InlineData("http://127.0.0.1")] // This is in the clients redirect URIs
        [InlineData("http://127.0.0.1:80")]
        [InlineData("http://127.0.0.1:1")]
        [InlineData("http://127.0.0.1:65536")]
        [InlineData("http://127.0.0.1:65536?HelloWorld=1234")]
        [InlineData("http://127.0.0.1:65536/hello/world")]
        public async Task Loopback_Redirect_URIs_Should_Be_AllowedAsync(string requestedUri)
        {
            var strictRedirectUriValidatorAppAuthValidator = new StrictRedirectUriValidatorAppAuth(TestLogger.Create<StrictRedirectUriValidatorAppAuth>());

            var result = await strictRedirectUriValidatorAppAuthValidator.IsRedirectUriValidAsync(requestedUri, clientWithValidLoopbackRedirectUri);

            result.Should().BeTrue();
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData("127.0.0.1")]
        [InlineData("//127.0.0.1")]
        [InlineData("http://127.0.0.1:t65536")]
        [InlineData("https://127.0.0.1")]
        public async Task Loopback_Redirect_URIs_Should_Not_Be_AllowedAsync(string requestedUri)
        {
            var strictRedirectUriValidatorAppAuthValidator = new StrictRedirectUriValidatorAppAuth(TestLogger.Create<StrictRedirectUriValidatorAppAuth>());

            var result = await strictRedirectUriValidatorAppAuthValidator.IsRedirectUriValidAsync(requestedUri, clientWithValidLoopbackRedirectUri);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_With_No_Redirect_Uris_Should_Not_Be_AllowedAsync()
        {
            var strictRedirectUriValidatorAppAuthValidator = new StrictRedirectUriValidatorAppAuth(TestLogger.Create<StrictRedirectUriValidatorAppAuth>());

            var result = await strictRedirectUriValidatorAppAuthValidator.IsRedirectUriValidAsync("http://127.0.0.1", clientWithNoRedirectUris);

            result.Should().BeFalse();
        }

    }
}
