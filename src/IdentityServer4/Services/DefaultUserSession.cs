using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.Services
{
    internal class DefaultUserSession : IUserSession
    {
        internal const string SessionIdKey = "session_id";
        internal const string ClientListKey = "client_list";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationSchemeProvider _schemes;
        private readonly IAuthenticationHandlerProvider _handlers;
        private readonly IdentityServerOptions _options;
        private readonly ISystemClock _clock;
        private readonly ILogger _logger;

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;
        private string CheckSessionCookieName => _options.Authentication.CheckSessionCookieName;

        private ClaimsPrincipal _principal;
        private AuthenticationProperties _properties;

        public DefaultUserSession(
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationSchemeProvider schemes,
            IAuthenticationHandlerProvider handlers,
            IdentityServerOptions options,
            ISystemClock clock,
            ILogger<IUserSession> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _schemes = schemes;
            _handlers = handlers;
            _options = options;
            _clock = clock;
            _logger = logger;
        }

        private async Task<string> GetCookieSchemeAsync()
        {
            var defaultScheme = await _schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultScheme == null)
            {
                throw new InvalidOperationException("No DefaultAuthenticateScheme found.");
            }

            return defaultScheme.Name;
        }

        // we need this helper (and can't call HttpContext.AuthenticateAsync) so we don't run 
        // claims transformation when we get the principal. this also ensures that we don't
        // re-issue a cookie that includes the claims from claims transformation.
        // 
        // also, by caching the _principal/_properties it allows someone to issue a new
        // cookie (via HttpContext.SignInAsync) and we'll use those new values, rather than
        // just reading the incoming cookie
        // 
        // this design requires this to be in DI as scoped
        private async Task AuthenticateAsync()
        {
            if (_principal == null || _properties == null)
            {
                var scheme = await GetCookieSchemeAsync();

                var handler = await _handlers.GetHandlerAsync(HttpContext, scheme);
                if (handler == null)
                {
                    throw new InvalidOperationException($"No authentication handler is configured to authenticate for the scheme: {scheme}");
                }

                var result = await handler.AuthenticateAsync();
                if (result != null && result.Succeeded)
                {
                    _principal = result.Principal;
                    _properties = result.Properties;
                }
            }
        }

        public async Task CreateSessionIdAsync(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var currentSubjectId = (await GetUserAsync())?.GetSubjectId();
            var newSubjectId = principal.GetSubjectId();

            if (!properties.Items.ContainsKey(SessionIdKey) || currentSubjectId != newSubjectId)
            {
                properties.Items[SessionIdKey] = CryptoRandom.CreateUniqueId(16);
            }

            IssueSessionIdCookie(properties.Items[SessionIdKey]);

            _principal = principal;
            _properties = properties;
        }

        public async Task<ClaimsPrincipal> GetUserAsync()
        {
            await AuthenticateAsync();

            return _principal;
        }

        public async Task<string> GetSessionIdAsync()
        {
            await AuthenticateAsync();

            if (_properties?.Items.ContainsKey(SessionIdKey) == true)
            {
                return _properties.Items[SessionIdKey];
            }

            return null;
        }

        public async Task EnsureSessionIdCookieAsync()
        {
            var sid = await GetSessionIdAsync();
            if (sid != null)
            {
                IssueSessionIdCookie(sid);
            }
            else
            {
                await RemoveSessionIdCookieAsync();
            }
        }

        public Task RemoveSessionIdCookieAsync()
        {
            if (HttpContext.Request.Cookies.ContainsKey(CheckSessionCookieName))
            {
                // only remove it if we have it in the request
                var options = CreateSessionIdCookieOptions();
                options.Expires = _clock.UtcNow.UtcDateTime.AddYears(-1);

                HttpContext.Response.Cookies.Append(CheckSessionCookieName, ".", options);
            }

            return Task.CompletedTask;
        }

        public async Task AddClientIdAsync(string clientId)
        {
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));

            var clients = await GetClientListAsync();
            if (!clients.Contains(clientId))
            {
                var update = clients.ToList();
                update.Add(clientId);

                await SetClientsAsync(update);
            }
        }

        public async Task<IEnumerable<string>> GetClientListAsync()
        {
            var value = await GetClientListPropertyValueAsync();
            try
            {
                return DecodeList(value);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error decoding client list: {0}", ex.Message);
                // clear so we don't keep failing
                await SetClientsAsync(null);
            }

            return Enumerable.Empty<string>();
        }

        // client list helpers
        private async Task<string> GetClientListPropertyValueAsync()
        {
            await AuthenticateAsync();

            if (_properties?.Items.ContainsKey(ClientListKey) == true)
            {
                return _properties.Items[ClientListKey];
            }

            return null;
        }

        private async Task SetClientsAsync(IEnumerable<string> clients)
        {
            var value = EncodeList(clients);
            await SetClientListPropertyValueAsync(value);
        }

        private async Task SetClientListPropertyValueAsync(string value)
        {
            await AuthenticateAsync();

            if (_principal == null || _properties == null) throw new InvalidOperationException("User is not currently authenticated");

            if (value == null)
            {
                _properties.Items.Remove(ClientListKey);
            }
            else
            {
                _properties.Items[ClientListKey] = value;
            }

            var scheme = await GetCookieSchemeAsync();
            await HttpContext.SignInAsync(scheme, _principal, _properties);
        }

        private IEnumerable<string> DecodeList(string value)
        {
            if (value.IsPresent())
            {
                var bytes = Base64Url.Decode(value);
                value = Encoding.UTF8.GetString(bytes);
                return ObjectSerializer.FromString<string[]>(value);
            }

            return Enumerable.Empty<string>();
        }

        private string EncodeList(IEnumerable<string> list)
        {
            if (list != null && list.Any())
            {
                var value = ObjectSerializer.ToString(list);
                var bytes = Encoding.UTF8.GetBytes(value);
                value = Base64Url.Encode(bytes);
                return value;
            }

            return null;
        }

        // session id cookie helpers
        private string GetSessionIdCookieValue()
        {
            return HttpContext.Request.Cookies[CheckSessionCookieName];
        }

        private void IssueSessionIdCookie(string sid)
        {
            if (GetSessionIdCookieValue() != sid)
            {
                HttpContext.Response.Cookies.Append(
                    _options.Authentication.CheckSessionCookieName,
                    sid,
                    CreateSessionIdCookieOptions());
            }
        }

        private CookieOptions CreateSessionIdCookieOptions()
        {
            var secure = HttpContext.Request.IsHttps;
            var path = HttpContext.GetIdentityServerBasePath().CleanUrlPath();

            var options = new CookieOptions
            {
                HttpOnly = false,
                Secure = secure,
                Path = path,
                SameSite = SameSiteMode.None
            };

            return options;
        }
    }
}
