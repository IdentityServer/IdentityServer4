using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    public class MockLogoutNotificationService : ILogoutNotificationService
    {
        public bool GetFrontChannelLogoutNotificationsUrlsCalled { get; set; }
        public List<string> FrontChannelLogoutNotificationsUrls { get; set; } = new List<string>();

        public Task<IEnumerable<string>> GetFrontChannelLogoutNotificationsUrlsAsync(LogoutNotificationContext context)
        {
            GetFrontChannelLogoutNotificationsUrlsCalled = true;
            return Task.FromResult(FrontChannelLogoutNotificationsUrls.AsEnumerable());
        }

        public bool SendBackChannelLogoutNotificationsCalled { get; set; }

        public Task SendBackChannelLogoutNotificationsAsync(LogoutNotificationContext context)
        {
            SendBackChannelLogoutNotificationsCalled = true;
            return Task.CompletedTask;
        }
    }
}
