using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http.Internal;
using Xunit;

namespace UnitTests.Hosting
{
    public class EndpointRouterTests
    {
        [Theory]
        [InlineData("/endpoint1")]
        [InlineData("endpoint1")]
        public void endpoints_should_be_bound_to_requests(string endpointPath)
        {
            var map = new Dictionary<string, Type> {{ endpointPath, typeof (MyEndpoint)}};
            var subject = new EndpointRouter(map);

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new Microsoft.AspNet.Http.PathString("/endpoint1");
            ctx.RequestServices = new StubServiceProvider();

            var result = subject.Find(ctx);
            result.Should().BeOfType<MyEndpoint>();
        }        

        private class MyEndpoint: IEndpoint
        {
            public Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
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