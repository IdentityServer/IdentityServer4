using FluentAssertions;
using IdentityServer4.Hosting.FederatedSignOut;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.UnitTests.Hosting
{
    public class FederatedSignoutTests
    {
        [Fact]
        public async void Client_with_multipe_endpoints_does_not_throw()
        {
            var inner = new AuthenticationRequestHandlerWrapper(new FakeAuthenticationHandler(), new FakeContextAccessor());
            var result = await inner.HandleRequestAsync();
            result.Should().BeFalse();
        }
    }

    class FakeRequestServices : IServiceProvider
    {
        public object GetService(Type serviceType) { return null; }
    }

    class FakeHttpContext : HttpContext
    {
        IServiceProvider _requestServices;
        public FakeHttpContext(IServiceProvider requestServices) { _requestServices = requestServices; }
        public override ConnectionInfo Connection => throw new NotImplementedException();
        public override IFeatureCollection Features => throw new NotImplementedException();
        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override HttpRequest Request => throw new NotImplementedException();
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => _requestServices; set => _requestServices = value; }
        public override HttpResponse Response => throw new NotImplementedException();
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override WebSocketManager WebSockets => throw new NotImplementedException();
        public override void Abort() { throw new NotImplementedException(); }
    }

    class FakeContextAccessor : IHttpContextAccessor
    {
        public FakeContextAccessor() { _context = new FakeHttpContext(new FakeRequestServices()); }
        HttpContext IHttpContextAccessor.HttpContext { get => _context; set => _context = value; }
        HttpContext _context;
    }

    class FakeAuthenticationHandler : IAuthenticationRequestHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync() { throw new NotImplementedException(); }
        public Task ChallengeAsync(AuthenticationProperties properties) { throw new NotImplementedException(); }
        public Task ForbidAsync(AuthenticationProperties properties) { throw new NotImplementedException(); }
        public Task<bool> HandleRequestAsync() { throw new NotImplementedException(); }
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) { throw new NotImplementedException(); }
    }
}
