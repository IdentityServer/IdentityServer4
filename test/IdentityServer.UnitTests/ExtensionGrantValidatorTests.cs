using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer4.UnitTests.Validation
{
    public class ExtensionGrantValidatorTests
    {
        private readonly ExtensionGrantValidator _sut;

        public ExtensionGrantValidatorTests()
        {
            _sut = new ExtensionGrantValidator(new[] {new TestGrantValidator(true)}, TestLogger.Create<ExtensionGrantValidator>());
        }

        [Fact]
        public async Task should_populate_username_property_in_extension_grant_validation_context()
        {
            var request = new ValidatedTokenRequest
            {
                Raw = new NameValueCollection
                {
                    { "username", "12345678" }
                },
                GrantType = "custom_grant"
            };

            var result = await _sut.ValidateAsync(request);

            result.Should().NotBeNull();
            request.UserName.Should().Be("12345678");
        }
    }
}