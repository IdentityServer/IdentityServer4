using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Infrastructure;
using IdentityServer4.Validation;

namespace IdentityServer4.UnitTests.Endpoints.Results
{
    class StubBackChannelLogoutClient : BackChannelLogoutClient
    {
        public StubBackChannelLogoutClient() : base(null, null, null, null)
        {
        }

        public bool SendLogoutsWasCalled { get; set; }

        public override Task SendLogoutsAsync(IEnumerable<BackChannelLogoutModel> clients)
        {
            SendLogoutsWasCalled = true;
            return Task.FromResult(0);
        }
    }
}
