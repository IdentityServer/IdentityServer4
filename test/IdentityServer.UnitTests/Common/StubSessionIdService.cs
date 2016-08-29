using IdentityServer4.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace IdentityServer4.UnitTests.Common
{
    public class StubSessionIdService : ISessionIdService
    {
        public string SessionId { get; set; } = "session_id";

        public Task AddSessionIdAsync(SignInContext context)
        {
            return Task.FromResult(0);
        }

        public string GetCookieName()
        {
            return "sessionid";
        }

        public string GetCookieValue()
        {
            return SessionId;
        }

        public Task<string> GetCurrentSessionIdAsync()
        {
            return Task.FromResult(SessionId);
        }

        public void RemoveCookie()
        {
            SessionId = null;
        }
    }
}
