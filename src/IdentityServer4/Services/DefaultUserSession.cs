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
    internal class DefaultUserSession : IUserSession
    {
        internal const string SessionIdKey = "session_id";
        internal const string ClientListKey = "client_list";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdentityServerOptions _options;
        private readonly ILogger _logger;

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;
        private string CheckSessionCookieName => _options.Authentication.CheckSessionCookieName;

        public DefaultUserSession(
            IHttpContextAccessor httpContextAccessor, 
            IdentityServerOptions options,
            ILogger<IUserSession> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _logger = logger;
        }

        public void CreateSessionId(AuthenticationProperties properties)
        {
            if (properties != null && !properties.Items.ContainsKey(SessionIdKey))
            {
                properties.Items[SessionIdKey] = CryptoRandom.CreateUniqueId(16);
            }

            IssueSessionIdCookie(properties.Items[SessionIdKey]);
        }

        public async Task<string> GetCurrentSessionIdAsync()
        {
            var info = await HttpContext.AuthenticateAsync();
            if (info != null && info.Properties != null)
            {
                if (info.Properties.Items.ContainsKey(SessionIdKey))
                {
                    return info.Properties.Items[SessionIdKey];
                }
            }

            return null;
        }

        public async Task EnsureSessionIdCookieAsync()
        {
            var sid = await GetCurrentSessionIdAsync();
            if (sid != null)
            {
                IssueSessionIdCookie(sid);
            }
            else
            {
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

        public async Task<ClaimsPrincipal> GetIdentityServerUserAsync()
        {
            return (await HttpContext.AuthenticateAsync()).Principal;
        }

        public async Task AddClientIdAsync(string clientId)
        {
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
        async Task<string> GetClientListPropertyValueAsync()
        {
            var info = await HttpContext.AuthenticateAsync();
            if (info == null)
            {
                _logger.LogWarning("No authenticated user");
                return null;
            }

            if (info.Properties.Items.ContainsKey(ClientListKey))
            {
                var value = info.Properties.Items[ClientListKey];
                return value;
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
            var info = await HttpContext.AuthenticateAsync();
            if (info == null || info.Principal == null)
            {
                _logger.LogError("No authenticated user");
                throw new InvalidOperationException("No authenticated user");
            }

            if (value == null)
            {
                info.Properties.Items.Remove(ClientListKey);
            }
            else
            {
                info.Properties.Items[ClientListKey] = value;
            }

            await HttpContext.SignInAsync(info.Principal, info.Properties);
        }

        public IEnumerable<string> DecodeList(string value)
        {
            if (value.IsPresent())
            {
                var bytes = Base64Url.Decode(value);
                value = Encoding.UTF8.GetString(bytes);
                return ObjectSerializer.FromString<string[]>(value);
            }

            return Enumerable.Empty<string>();
        }

        public string EncodeList(IEnumerable<string> list)
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
