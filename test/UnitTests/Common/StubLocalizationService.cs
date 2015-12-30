using IdentityServer4.Core.Services;

namespace UnitTests.Common
{
    public class StubLocalizationService : ILocalizationService
    {
        public string Result { get; set; }

        public string GetString(string category, string id)
        {
            return Result;
        }
    }
}
