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
    // todo: review
    // 
    // needs to be scoped in DI since the design is to hold current user as state
    internal class DefaultUserSession : IUserSession
    {
        internal const string SessionIdKey = "session_id";
        internal const string ClientListKey = "client_list";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdentityServerOptions _options;
        private readonly ILogger _logger;

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;
        private string CheckSessionCookieName => _options.Authentication.CheckSessionCookieName;

        private ClaimsPrincipal _principal;
        private AuthenticationProperties _properties;

        public DefaultUserSession(
            IHttpContextAccessor httpContextAccessor,
            IdentityServerOptions options,
            ILogger<IUserSession> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _logger = logger;
        }

        public void CreateSessionId(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var currentSubjectId = _principal?.GetSubjectId();
            var newSubjectId = principal.GetSubjectId();

            if (currentSubjectId == null || currentSubjectId != newSubjectId)
            {
                properties.Items[SessionIdKey] = CryptoRandom.CreateUniqueId(16);
            }

            IssueSessionIdCookie(properties.Items[SessionIdKey]);

            SetCurrentUser(principal, properties);
        }

        public void SetCurrentUser(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            _principal = principal ?? throw new ArgumentNullException(nameof(principal));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public ClaimsPrincipal User => _principal;
        public string SessionId
        {
            get
            {
                if (_properties?.Items.ContainsKey(SessionIdKey) == true)
                {
                    return _properties.Items[SessionIdKey];
                }

                return null;
            }
        }

        public void EnsureSessionIdCookie()
        {
            var sid = SessionId;
            if (sid != null)
            {
                IssueSessionIdCookie(sid);
            }
            else
            {
                // todo: validate if this is still true since we now pass a 
                // seralized message as param to end sesison callback
                //
                // we don't want to delete the session id cookie if the user is
                // no longer authenticated since we might be waiting for the 
                // signout iframe to render -- it's a timing issue between the 
                // logout page removing the authentication cookie and the 
                // signout iframe callback from performing SLO
            }
        }

        public void RemoveSessionIdCookie()
        {
            if (HttpContext.Request.Cookies.ContainsKey(CheckSessionCookieName))
            {
                // only remove it if we have it in the request
                var options = CreateSessionIdCookieOptions();
                options.Expires = _options.UtcNow.AddYears(-1);

                HttpContext.Response.Cookies.Append(CheckSessionCookieName, ".", options);
            }
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
            var value = GetClientListPropertyValue();
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
        string GetClientListPropertyValue()
        {
            if (_properties?.Items.ContainsKey(ClientListKey) == true)
            {
                return _properties.Items[ClientListKey];
            }

            return null;
        }

        async Task SetClientsAsync(IEnumerable<string> clients)
        {
            var value = EncodeList(clients);
            await SetClientListPropertyValueAsync(value);
        }

        async Task SetClientListPropertyValueAsync(string value)
        {
            if (_principal == null || _properties == null) throw new InvalidOperationException("User is not currently authenticated");

            if (value == null)
            {
                _properties.Items.Remove(ClientListKey);
            }
            else
            {
                _properties.Items[ClientListKey] = value;
            }

            await HttpContext.SignInAsync(_principal, _properties);
        }

        IEnumerable<string> DecodeList(string value)
        {
            if (value.IsPresent())
            {
                var bytes = Base64Url.Decode(value);
                value = Encoding.UTF8.GetString(bytes);
                return ObjectSerializer.FromString<string[]>(value);
            }

            return Enumerable.Empty<string>();
        }

        string EncodeList(IEnumerable<string> list)
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
        string GetSessionIdCookieValue()
        {
            return HttpContext.Request.Cookies[CheckSessionCookieName];
        }

        void IssueSessionIdCookie(string sid)
        {
            if (GetSessionIdCookieValue() != sid)
            {
                HttpContext.Response.Cookies.Append(
                    _options.Authentication.CheckSessionCookieName,
                    sid,
                    CreateSessionIdCookieOptions());
            }
        }

        CookieOptions CreateSessionIdCookieOptions()
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
