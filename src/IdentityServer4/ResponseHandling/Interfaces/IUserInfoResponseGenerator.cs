using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.ResponseHandling
{
    public interface IUserInfoResponseGenerator
    {
        Task<Dictionary<string, object>> ProcessAsync(string subject, IEnumerable<string> scopes, Client client);
    }
}