// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Hosting;
using Xunit;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using IdentityServer4.UnitTests.Common;

namespace IdentityServer4.UnitTests.Hosting
{
    public class EndpointRouterTests
    {
        Dictionary<string, EndpointName> _pathMap;
        List<EndpointMapping> _mappings;
        IdentityServerOptions _options;
        EndpointRouter _subject;

        public EndpointRouterTests()
        {
            _pathMap = new Dictionary<string, EndpointName>();
            _mappings = new List<EndpointMapping>();
            _options = new IdentityServerOptions();
            _subject = new EndpointRouter(_pathMap, _options, _mappings, TestLogger.Create<EndpointRouter>());
        }

        [Fact]
        public void Find_should_return_null_for_incorrect_path()
        {
            _pathMap.Add("/endpoint", EndpointName.Authorize);
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyEndpoint) });

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/wrong");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeNull();
        }

        [Fact]
        public void Find_should_find_path()
        {
            _pathMap.Add("/endpoint", EndpointName.Authorize);
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyEndpoint) });

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/endpoint");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeOfType<MyEndpoint>();
        }

        [Fact]
        public void Find_should_find_unprefixed_path()
        {
            _pathMap.Add("endpoint", EndpointName.Authorize);
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyEndpoint) });

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/endpoint");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeOfType<MyEndpoint>();
        }

        [Fact]
        public void Find_should_find_nested_paths()
        {
            _pathMap.Add("/endpoint", EndpointName.Authorize);
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyEndpoint) });

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/endpoint/subpath");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeOfType<MyEndpoint>();
        }

        [Fact]
        public void Find_should_find_last_registered_mapping()
        {
            _pathMap.Add("/endpoint", EndpointName.Authorize);
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyEndpoint) });
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyOtherEndpoint) });

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/endpoint");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeOfType<MyOtherEndpoint>();
        }

        [Fact]
        public void Find_should_return_null_for_disabled_endpoint()
        {
            _pathMap.Add("/endpoint", EndpointName.Authorize);
            _mappings.Add(new EndpointMapping { Endpoint = EndpointName.Authorize, Handler = typeof(MyEndpoint) });
            _options.Endpoints.EnableAuthorizeEndpoint = false;

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/endpoint");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeNull();
        }

        private class MyEndpoint: IEndpoint
        {
            public Task<IEndpointResult> ProcessAsync(HttpContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class MyOtherEndpoint : IEndpoint
        {
            public Task<IEndpointResult> ProcessAsync(HttpContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class StubServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(MyEndpoint)) return new MyEndpoint();
                if (serviceType == typeof(MyOtherEndpoint)) return new MyOtherEndpoint();

                throw new InvalidOperationException();
            }
        }
    }
}