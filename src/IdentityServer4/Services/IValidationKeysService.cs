using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public interface IValidationKeysStore
    {
        Task<IEnumerable<SecurityKey>> GetValidationKeysAsync();
    }
}
