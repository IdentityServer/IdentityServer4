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
    internal class AuthenticationRequestSignInHandlerWrapper : AuthenticationRequestSignOutHandlerWrapper, IAuthenticationSignInHandler
    {
        private readonly IAuthenticationSignInHandler _inner;

        public AuthenticationRequestSignInHandlerWrapper(IAuthenticationSignInHandler inner, IHttpContextAccessor httpContextAccessor)
            : base(inner, httpContextAccessor)
        {
            _inner = inner;
        }

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            return _inner.SignInAsync(user, properties);
        }
    }
}
