using IdentityServer4.Core.Services;
using System.Threading.Tasks;
using IdentityServer4.Core.Events;

namespace UnitTests.Common
{
    public class FakeEventService : IEventService
    {
        public Task RaiseAsync<T>(Event<T> evt)
        {
            return Task.FromResult(0);
        }
    }
}
