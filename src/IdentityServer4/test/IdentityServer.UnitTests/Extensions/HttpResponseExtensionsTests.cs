// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace IdentityServer4.UnitTests.Extensions
{
    public class HttpResponseExtensionsTests
    {
        [Fact]
        public void AddScriptCspHeaders_default()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions();
            httpResponse.AddScriptCspHeaders(
                cspOptions,
                "bar"
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; script-src 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddScriptCspHeaders_with_csp_level_1()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {Level = CspLevel.One};
            httpResponse.AddScriptCspHeaders(
                cspOptions,
                "bar"
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; script-src 'unsafe-inline' 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddScriptCspHeaders_without_deprecated_header()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {AddDeprecatedHeader = false};
            httpResponse.AddScriptCspHeaders(
                cspOptions,
                "bar"
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; script-src 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().BeEmpty();
        }

        [Fact]
        public void AddScriptCspHeaders_with_user_default_src()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {DefaultSrc = "data:"};
            httpResponse.AddScriptCspHeaders(
                cspOptions,
                "bar"
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none' data:; script-src 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddScriptCspHeaders_with_user_script_src()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {ScriptSrc = "data:"};
            httpResponse.AddScriptCspHeaders(
                cspOptions,
                "bar"
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; script-src 'bar' data:"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddStyleCspHeaders_default()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions();
            httpResponse.AddStyleCspHeaders(
                cspOptions,
                "bar",
                null
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; style-src 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddStyleCspHeaders_with_csp_level_1()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {Level = CspLevel.One};
            httpResponse.AddStyleCspHeaders(
                cspOptions,
                "bar",
                null
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; style-src 'unsafe-inline' 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddStyleCspHeaders_with_frame_src()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions();
            httpResponse.AddStyleCspHeaders(
                cspOptions,
                "bar",
                "baz"
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; style-src 'bar'; frame-src baz"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddStyleCspHeaders_without_deprecated_header()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {AddDeprecatedHeader = false};
            httpResponse.AddStyleCspHeaders(
                cspOptions,
                "bar",
                null
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; style-src 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().BeEmpty();
        }

        [Fact]
        public void AddStyleCspHeaders_with_user_default_src()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {DefaultSrc = "data:"};
            httpResponse.AddStyleCspHeaders(
                cspOptions,
                "bar",
                null
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none' data:; style-src 'bar'"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        [Fact]
        public void AddStyleCspHeaders_with_user_style_src()
        {
            var httpResponse = new HttpResponseMock();

            var cspOptions = new CspOptions {StyleSrc = "data:"};
            httpResponse.AddStyleCspHeaders(
                cspOptions,
                "bar",
                null
            );

            httpResponse.Headers["Content-Security-Policy"].Should().Contain(
                "default-src 'none'; style-src 'bar' data:"
            );
            httpResponse.Headers["X-Content-Security-Policy"].Should().Contain(
                httpResponse.Headers["Content-Security-Policy"]
            );
        }

        class HttpResponseMock : HttpResponse
        {
            public override void OnCompleted(Func<object, Task> callback, object state)
            {
                throw new NotImplementedException();
            }

            public override void OnStarting(Func<object, Task> callback, object state)
            {
                throw new NotImplementedException();
            }

            public override void Redirect(string location, bool permanent)
            {
                throw new NotImplementedException();
            }

            public override Stream Body { get; set; }
            public override long? ContentLength { get; set; }
            public override string ContentType { get; set; }
            public override IResponseCookies Cookies { get; }
            public override bool HasStarted { get; }
            public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
            public override HttpContext HttpContext { get; }
            public override int StatusCode { get; set; }
        }
    }
}