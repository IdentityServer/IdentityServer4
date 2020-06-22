using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    class MockKeyMaterialService : IKeyMaterialService
    {
        public List<SigningCredentials> SigningCredentials = new List<SigningCredentials>();
        public List<SecurityKeyInfo> ValidationKeys = new List<SecurityKeyInfo>();

        public Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SigningCredentials.AsEnumerable());
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SigningCredentials.FirstOrDefault());
        }

        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ValidationKeys.AsEnumerable());
        }
    }
}
