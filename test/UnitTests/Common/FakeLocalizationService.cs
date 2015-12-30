using IdentityServer4.Core.Services;

namespace UnitTests.Common
{
    public class FakeLocalizationService : ILocalizationService
    {
        public string GetString(string category, string id)
        {
            return null;
        }
    }
}
