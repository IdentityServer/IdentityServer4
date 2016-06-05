using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public interface ISigningCredentialStore
    {
        Task<SigningCredentials> GetSigningCredentialsAsync();
    }
}