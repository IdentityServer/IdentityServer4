using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public static class ValidationKeyStoreExtensions
    {
        public static async Task<IEnumerable<SecurityKey>> GetKeysAsync(this IEnumerable<IValidationKeysStore> stores)
        {
            var keys = new List<SecurityKey>();

            foreach (var store in stores)
            {
                keys.AddRange(await store.GetValidationKeysAsync());
            }

            return keys;
        }
    }
}