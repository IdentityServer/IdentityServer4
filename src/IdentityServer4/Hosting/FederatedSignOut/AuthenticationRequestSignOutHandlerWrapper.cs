using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Configuration.DependencyInjection;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting.FederatedSignOut
{
    internal class AuthenticationRequestSignOutHandlerWrapper : AuthenticationRequestHandlerWrapper, IAuthenticationSignOutHandler
    {
        private readonly IAuthenticationSignOutHandler _inner;

        public AuthenticationRequestSignOutHandlerWrapper(IAuthenticationSignOutHandler inner, IHttpContextAccessor httpContextAccessor)
            : base((IAuthenticationRequestHandler)inner, httpContextAccessor)
        {
            _inner = inner;
        }

        public Task SignOutAsync(AuthenticationProperties properties)
        {
            return _inner.SignOutAsync(properties);
        }
    }
}
