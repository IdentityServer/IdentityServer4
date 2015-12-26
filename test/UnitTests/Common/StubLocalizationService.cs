using IdentityServer4.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
