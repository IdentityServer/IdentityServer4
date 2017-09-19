using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Configuration.DependencyInjection;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting
{
    // todo: FML
    // this intercepts IAuthenticationRequestHandler authentication handlers
    // to detect when they are handling federated signout. when they are invoked,
    // call signout on the default authentication scheme, and return 200 then 
    // we assume they are handling the federated signout in an iframe. 
    // based on this assumption, we then render our federated signout iframes 
    // to any current clients.
    internal class FederatedSignoutAuthenticationHandlerProvider : IAuthenticationHandlerProvider
    {
        private readonly IAuthenticationHandlerProvider _provider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FederatedSignoutAuthenticationHandlerProvider(
            Decorator<IAuthenticationHandlerProvider> decorator, 
            IHttpContextAccessor httpContextAccessor)
        {
            _provider = decorator.Instance;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IAuthenticationHandler> GetHandlerAsync(HttpContext context, string authenticationScheme)
        {
            var handler = await _provider.GetHandlerAsync(context, authenticationScheme);
            if (handler is IAuthenticationRequestHandler requestHandler)
            {
                if (requestHandler is IAuthenticationSignInHandler signinHandler)
                {
                    return new AuthenticationRequestSignInHandlerWrapper(signinHandler, _httpContextAccessor);
                }

                if (requestHandler is IAuthenticationSignOutHandler signoutHandler)
                {
                    return new AuthenticationRequestSignOutHandlerWrapper(signoutHandler, _httpContextAccessor);
                }

                return new AuthenticationRequestHandlerWrapper(requestHandler, _httpContextAccessor);
            }

            return handler;
        }
    }

    internal class AuthenticationRequestHandlerWrapper : IAuthenticationRequestHandler
    {
        private const string DocumentHtml = "<!DOCTYPE html><html><body>{0}</body></html>";
        private const string IframeHtml = "<iframe style='display:none' width='0' height='0' src='{0}'></iframe>";

        private readonly IAuthenticationRequestHandler _inner;
        private readonly HttpContext _context;
        private readonly ILogger _logger;

        public AuthenticationRequestHandlerWrapper(IAuthenticationRequestHandler inner, IHttpContextAccessor httpContextAccessor)
        {
            _inner = inner;
            _context = httpContextAccessor.HttpContext;

            var factory = (ILoggerFactory)_context.RequestServices.GetService(typeof(ILoggerFactory));
            _logger = factory?.CreateLogger(GetType());
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            return _inner.InitializeAsync(scheme, context);
        }

        public async Task<bool> HandleRequestAsync()
        {
            var result = await _inner.HandleRequestAsync();

            if (result && _context.GetSignOutCalled() && _context.Response.StatusCode == 200)
            {
                // given that this runs prior to the authentication middleware running
                // we need to explicitly trigger authentication so we can have our 
                // session service populated with the current user info
                await _context.AuthenticateAsync();

                // now we can do our processing to render the iframe (if needed)
                await ProcessFederatedSignOutRequestAsync();
            }

            return result;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            return _inner.AuthenticateAsync();
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            return _inner.ChallengeAsync(properties);
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            return _inner.ForbidAsync(properties);
        }

        private async Task ProcessFederatedSignOutRequestAsync()
        {
            _logger?.LogDebug("Processing federated signout");

            var iframeUrl = await _context.GetIdentityServerSignoutFrameCallbackUrlAsync();
            if (iframeUrl != null)
            {
                _logger?.LogDebug("Rendering signout callback iframe");
                await RenderResponseAsync(iframeUrl);
            }
            else
            {
                _logger?.LogDebug("No signout callback iframe to render");
            }
        }

        private async Task RenderResponseAsync(string iframeUrl)
        {
            _context.Response.SetNoCache();

            if (_context.Response.Body.CanWrite)
            {
                var iframe = String.Format(IframeHtml, iframeUrl);
                var doc = String.Format(DocumentHtml, iframe);
                _context.Response.ContentType = "text/html";
                await _context.Response.WriteAsync(doc);
            }
        }
    }

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
