using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    public interface ISigningCredentialStore
    {
        Task<SigningCredentials> GetSigningCredentialsAsync();
    }
}
