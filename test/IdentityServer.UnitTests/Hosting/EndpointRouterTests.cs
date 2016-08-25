using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Hosting;
using Xunit;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;

namespace UnitTests.Hosting
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
            _subject = new EndpointRouter(_pathMap, _options, _mappings);
        }

        [Theory(Skip = "rework for new structure")]
        [InlineData("/endpoint1")]
        [InlineData("endpoint1")]
        public void todo(string endpointPath)
        {
            //_subject.Find();

            //    var map = new Dictionary<string, Type> { { endpointPath, typeof(MyEndpoint) } };
            //    var mappings = new List<EndpointMapping> { new EndpointMapping {   };
            //    var subject = new EndpointRouter(map);

            //    var ctx = new DefaultHttpContext();
            //    ctx.Request.Path = new PathString("/endpoint1");
            //    ctx.RequestServices = new StubServiceProvider();

            //    var result = subject.Find(ctx);
            //    result.Should().BeOfType<MyEndpoint>();
        }

        private class MyEndpoint: IEndpoint
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
                return serviceType == typeof (MyEndpoint) ? new MyEndpoint() : null;
            }
        }
    }
}