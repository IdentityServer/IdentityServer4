using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    public interface ISigningCredentialStore
    {
        Task<SigningCredentials> GetSigningCredentialsAsync();
    }
}