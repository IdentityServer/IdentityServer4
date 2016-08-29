﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.Hosting.Cors;
using IdentityServer4.UnitTests.Common;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System;
using IdentityServer4.Configuration;

namespace IdentityServer4.UnitTests.Hosting.Cors
{
    public class FakePolicyProvider : ICorsPolicyProvider
    {
        public Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
        {
            return Task.FromResult<CorsPolicy>(null);
        }
    }

    public class PolicyProviderTests
    {
        const string Category = "PolicyProvider";

        PolicyProvider<FakePolicyProvider> _subject;
        List<string> _allowedPaths = new List<string>();

        MockCorsPolicyService _mockPolicy = new MockCorsPolicyService();
        IdentityServerOptions _options;

        public PolicyProviderTests()
        {
            Init();
        }

        public void Init()
        {
            _options = new IdentityServerOptions();
            _options.CorsOptions.CorsPaths.Clear();
            foreach(var path in _allowedPaths)
            {
                _options.CorsOptions.CorsPaths.Add(new PathString(path));
            }

            _subject = new PolicyProvider<FakePolicyProvider>(
                TestLogger.Create<PolicyProvider<FakePolicyProvider>>(),
                new FakePolicyProvider(),
                _options,
                _mockPolicy);
        }

        [Theory]
        [InlineData("/foo")]
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
            ctx.Request.Host = new HostString("server");
            ctx.Request.Path = new PathString(path);
            ctx.Request.Headers.Add("Origin", "http://notserver");

            var response = await _subject.GetPolicyAsync(ctx, _options.CorsOptions.CorsPolicyName);

            _mockPolicy.WasCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData("/foo/")]
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
            ctx.Request.Host = new HostString("server");
            ctx.Request.Path = new PathString(path);
            ctx.Request.Headers.Add("Origin", "http://notserver");

            var response = await _subject.GetPolicyAsync(ctx, _options.CorsOptions.CorsPolicyName);

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
            ctx.Request.Host = new HostString("server");
            ctx.Request.Path = new PathString("/foo");
            ctx.Request.Headers.Add("Origin", "https://server");

            var response = await _subject.GetPolicyAsync(ctx, _options.CorsOptions.CorsPolicyName);

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
            ctx.Request.Host = new HostString("server");
            ctx.Request.Path = new PathString("/foo");
            ctx.Request.Headers.Add("Origin", origin);

            var response = await _subject.GetPolicyAsync(ctx, _options.CorsOptions.CorsPolicyName);

            _mockPolicy.WasCalled.Should().BeTrue();
        }
    }
}
