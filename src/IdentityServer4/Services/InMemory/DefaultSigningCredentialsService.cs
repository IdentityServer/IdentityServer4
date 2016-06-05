using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.InMemory
{
    public class DefaultSigningCredentialsStore : ISigningCredentialStore
    {
        private readonly SigningCredentials _credential;

        public DefaultSigningCredentialsStore(SigningCredentials credential)
        {
            _credential = credential;
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return Task.FromResult(_credential);
        }
    }
}