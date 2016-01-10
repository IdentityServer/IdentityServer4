using FluentAssertions;
using IdentityServer4.Core.Hosting.Cors;
using Microsoft.AspNet.Http.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Hosting.Cors
{
    public class PolicyProviderTests
    {
        const string Category = "PolicyProvider";

        PolicyProvider _subject;
        List<string> _allowedPaths = new List<string>();

        MockCorsPolicyService _mockPolicy = new MockCorsPolicyService();

        public PolicyProviderTests()
        {
            Init();
        }

        public void Init()
        {
            _subject = new PolicyProvider(
                new FakeLogger<PolicyProvider>(),
                _allowedPaths,
                _mockPolicy);
        }

        [Theory]
        [InlineData("/foo")]
        [InlineData("/foo/")]
        [InlineData("/bar")]
        [InlineData("/bar/")]
        [InlineData("/baz/quux")]
        [InlineData("/baz/quux/")]
        [Trait("Category", Category)]
        public async Task valid_paths_should_call_policy_service(string path)
        {
            _allowedPaths.AddRange(new string[] {
                "/foo",
                "/bar/",
                "/baz/quux",
                "/baz/quux/",
            });
            Init();

            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            ctx.Request.Host = new Microsoft.AspNet.Http.HostString("server");
            ctx.Request.Path = new Microsoft.AspNet.Http.PathString(path);
            ctx.Request.Headers.Add("Origin", "http://notserver");

            var response = await _subject.GetPolicyAsync(ctx, null);

            _mockPolicy.WasCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData("/xoxo")]
        [InlineData("/xoxo/")]
        [InlineData("/foo/xoxo")]
        [InlineData("/baz/quux/xoxo")]
        [Trait("Category", Category)]
        public async Task invalid_paths_should_not_call_policy_service(string path)
        {
            _allowedPaths.AddRange(new string[] {
                "/foo",
                "/bar",
                "/baz/quux"
            });
            Init();

            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            ctx.Request.Host = new Microsoft.AspNet.Http.HostString("server");
            ctx.Request.Path = new Microsoft.AspNet.Http.PathString(path);
            ctx.Request.Headers.Add("Origin", "http://notserver");

            var response = await _subject.GetPolicyAsync(ctx, null);

            _mockPolicy.WasCalled.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task origin_same_as_server_should_not_call_policy()
        {
            _allowedPaths.AddRange(new string[] {
                "/foo",
            });
            Init();

            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            ctx.Request.Host = new Microsoft.AspNet.Http.HostString("server");
            ctx.Request.Path = new Microsoft.AspNet.Http.PathString("/foo");
            ctx.Request.Headers.Add("Origin", "https://server");

            var response = await _subject.GetPolicyAsync(ctx, null);

            _mockPolicy.WasCalled.Should().BeFalse();
        }

        [Theory]
        [InlineData("https://notserver")]
        [InlineData("http://server")]
        [Trait("Category", Category)]
        public async Task origin_not_same_as_server_should_call_policy(string origin)
        {
            _allowedPaths.AddRange(new string[] {
                "/foo",
            });
            Init();

            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            ctx.Request.Host = new Microsoft.AspNet.Http.HostString("server");
            ctx.Request.Path = new Microsoft.AspNet.Http.PathString("/foo");
            ctx.Request.Headers.Add("Origin", origin);

            var response = await _subject.GetPolicyAsync(ctx, null);

            _mockPolicy.WasCalled.Should().BeTrue();
        }

    }
}
