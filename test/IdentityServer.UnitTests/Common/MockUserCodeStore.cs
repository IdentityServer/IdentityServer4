using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common
{
    public class MockUserCodeStore : IUserCodeStore
    {
        public Dictionary<string, UserCode> Codes { get; set; } = new Dictionary<string, UserCode>();

        public Task StoreUserCodeAsync(string code, UserCode data)
        {
            Codes[code] = data;
            return Task.CompletedTask;
        }

        public Task<UserCode> GetUserCodeAsync(string code)
        {
            UserCode val = null;
            if (code != null)
            {
                Codes.TryGetValue(code, out val);
            }
            return Task.FromResult(val);
        }

        public Task RemoveUserCodeAsync(string code)
        {
            if (code != null && Codes.ContainsKey(code))
            {
                Codes.Remove(code);
            }
            return Task.CompletedTask;
        }
    }
}