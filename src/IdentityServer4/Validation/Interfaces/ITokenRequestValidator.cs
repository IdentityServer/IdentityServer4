using System.Collections.Specialized;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Validation
{
    public interface ITokenRequestValidator
    {
        Task<TokenRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, Client client);
    }
}