
using FluentAssertions;
using IdentityServer4.Core.Services.Default;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Services.Default
{
    public class DefaultCorsPolicyServiceTests
    {
        const string Category = "DefaultCorsPolicyService";

        DefaultCorsPolicyService subject;

        public DefaultCorsPolicyServiceTests()
        {
            subject = new DefaultCorsPolicyService(new FakeLogger<DefaultCorsPolicyService>());
        }

        [Fact]
        [Trait("Category", Category)]
        public void IsOriginAllowed_OriginIsAllowed_ReturnsTrue()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.IsOriginAllowedAsync("http://foo").Result.Should().Be(true);
        }

        [Fact]
        [Trait("Category", Category)]
        public void IsOriginAllowed_OriginIsNotAllowed_ReturnsFalse()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.IsOriginAllowedAsync("http://bar").Result.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public void IsOriginAllowed_OriginIsInAllowedList_ReturnsTrue()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.AllowedOrigins.Add("http://bar");
            subject.AllowedOrigins.Add("http://baz");
            subject.IsOriginAllowedAsync("http://bar").Result.Should().Be(true);
        }

        [Fact]
        [Trait("Category", Category)]
        public void IsOriginAllowed_OriginIsNotInAllowedList_ReturnsFalse()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.AllowedOrigins.Add("http://bar");
            subject.AllowedOrigins.Add("http://baz");
            subject.IsOriginAllowedAsync("http://quux").Result.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public void IsOriginAllowed_AllowAllTrue_ReturnsTrue()
        {
            subject.AllowAll = true;
            subject.IsOriginAllowedAsync("http://foo").Result.Should().Be(true);
        }
    }
}
