using IdentityServer4.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
