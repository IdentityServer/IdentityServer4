using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public interface IValidationKeysStore
    {
        Task<IEnumerable<SecurityKey>> GetValidationKeysAsync();
    }
}